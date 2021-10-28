using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;

using SonicBloom.Koreo;

/// <summary>
/// 点击输入处理流程：
/// 由 <c>Track</c> 负责在特定时间生成 <c>Note</c> ，并获取玩家输入。
/// 当检测到玩家输入时，计算当前还在轨道上的最早的 <c>Note</c>，并对延迟进行判定。
/// 判定结果通过直接调用 <c>Note.UpdateVerdict</c> 通知 <c>Note</c>。
/// <c>Note</c> 接收到判定结果并播放相应的动画。
/// <c>NoteGroup</c> 会监听组内 <c>Note</c> 的判定结果，
/// 如果 <c>Note</c> 接收到 Miss 判定，则会直接给 <c>NoteGroup</c> 中的所有
/// </summary>
public class Track : MonoBehaviour
{
    public int TrackId;
    public KeyCode Key;
    public GameObject SingleNoteObject, HoldNoteObject;
    public GameObject NoteLink;

    public Koreography koreography;
    public KoreographyTrack InstantiateEventTrack;
    public KoreographyTrack MissEventTrack;

    public List<Note> Notes;
    public Dictionary<int, NoteGroup> NoteGroups;

    public Action<object, NoteVerdict> HandleVerdict;
    public Vector3 TrackEndPoint
    {
        get => noteLink.EndPoint;
    }

    private NoteInTrack noteInTrack;
    private NoteLink noteLink;

    void Start()
    {
        Assert.IsNotNull(koreography, $"Koreography of track {TrackId} has not been set");
        Assert.IsNotNull(InstantiateEventTrack, $"Track of instantiate event of #{TrackId} has not been set");
        Assert.IsNotNull(MissEventTrack, $"Track of instantiate event of #{TrackId} has not been set");

        // Initialize parameters
        noteInTrack = new NoteInTrack(Notes);
        var noteLinkObject = GameObject.Instantiate(NoteLink);
        noteLink = noteLinkObject.GetComponent<NoteLink>();
        noteLink.koreography = koreography;
        Assert.IsNotNull(noteLink);

        Koreographer.Instance.RegisterForEvents(InstantiateEventTrack.EventID, InstanciateNote);
        Koreographer.Instance.RegisterForEvents(MissEventTrack.EventID, HandleNoteMiss);
    }

    void Update()
    {
        if (Input.GetKeyDown(Key))
        {
            Click(true);
        }
        else if (Input.GetKey(Key))
        {
            Click(false);
        }
    }

    void OnDestroy()
    {
        koreography?.RemoveTrack(InstantiateEventTrack);
        koreography?.RemoveTrack(MissEventTrack);
        Koreographer.Instance?.UnregisterForAllEvents(this);
    }

    /// <summary>
    /// 根据事件中所带的 noteID 在游戏中生成对应的 <c>Note</c> 和 <c>NoteLink</c>
    /// </summary>
    void InstanciateNote(KoreographyEvent evt)
    {
        var note = Notes[evt.GetIntValue()];
        if (note.Instantiated) {
            Debug.LogWarning($"Double instanciate of Note #{note.Info.ToString()}.");
            return;
        }
        note.Instantiated = true;

        GameObject noteObject;
        switch (note.Info.NoteType)
        {
        case NoteType.Single:
            noteObject = GameObject.Instantiate(SingleNoteObject);
            note.SetNoteObject(noteObject);
            break;
        case NoteType.Hold:
            noteObject = GameObject.Instantiate(HoldNoteObject);
            note.SetNoteObject(noteObject);
            break;
        default:
            return;
        }

        if (note.Info.Group != 0)
        {
            var group = NoteGroups[note.Info.Group];
            note.OnHasVerdict += group.UpdateVerdict;
            group.OnHasVerdict += HandleVerdict;
        }
        else
        {
            note.OnHasVerdict += HandleVerdict;
        }

        noteLink.Add(note);
    }

    public void HandleNoteMiss(KoreographyEvent evt)
    {
        var note = Notes[evt.GetIntValue()];
        if (note.Verdict == null) {
            Debug.Log($"{koreography.GetLatestSampleTime()}, {note.Info.ShouldHitAtSample}");
            note.Verdict = new NoteVerdict((int)Config.MaxAdvanceHit.TotalMilliseconds);
        }
    }

    public bool Click(bool isBegining)
    {
        var currentSample = koreography.GetLatestSampleTime();
        var noteAndOffset = noteInTrack.GetCurrentNoteAndSample(currentSample);
        if (noteAndOffset == null)
            return false;
        var (note, offset) = ((Note, int))noteAndOffset;

        switch (note.Info.NoteType)
        {
        case NoteType.Single:
            if (isBegining)
            {
                note.Verdict = JudgeNote(offset);
            }
            return true;
        case NoteType.Hold:
            if (offset >= 0)
            {
                note.Verdict = JudgeNote(offset);
            }
            return true;
        }
        return false;
    }

    NoteVerdict JudgeNote(int offsetSample)
    {
        var offsetMs = offsetSample * 1000 / koreography.SampleRate;
        return new NoteVerdict(offsetMs);
    }
}

public class NoteInTrack
{
    public NoteInTrack(List<Note> notes)
    {
        foreach (var note in notes)
            this.notes.Add(note.Info.ShouldHitAtSample, note);
    }

    /// <summary>
    /// 更新当前时间至 currentSample ，处理 "too late" 的 miss 并且获取最后未点击的 note
    /// </summary>
    public (Note, int)? GetCurrentNoteAndSample(int currentSample)
    {
        while (notes.Count > 0)
        {
            var kvPair = notes.First();
            var hitAtSample = kvPair.Key;
            var note = kvPair.Value;
            var offset = currentSample - hitAtSample;

            if (note.Verdict != null)
            {
                notes.Remove(hitAtSample);
                continue;
            }

            if (offset > -Config.MaxAdvanceHitSample)
            {
                notes.Remove(hitAtSample);
                return (note, offset);
            }
            // Too early, ignored
            return null;
        }
        return null;
    }

    private SortedDictionary<int, Note> notes = new SortedDictionary<int, Note>();
}
