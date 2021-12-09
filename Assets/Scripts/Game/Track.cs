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
    [NonSerialized]
    public int TrackId;

    public KeyCode Key;
    public GameObject SingleNoteObject, HoldNoteObject;
    public GameObject NoteLink;

    Koreography koreography;
    KoreographyTrack InstantiateEventTrack;
    KoreographyTrack MissEventTrack;

    bool AutoPlay = true;
    KoreographyTrack AutoPlayEventTrack;

    List<Note> Notes;
    Dictionary<int, NoteGroup> NoteGroups;

    public Vector3 TrackEndPoint
    {
        get => noteLink.EndPoint;
    }

    NoteInTrack noteInTrack;
    NoteLink noteLink;

    void Start()
    {
        var noteLinkObject = GameObject.Instantiate(NoteLink);
        noteLink = noteLinkObject.GetComponent<NoteLink>();
        Assert.IsNotNull(noteLink);
    }

    public void Init(List<Note> Notes, Dictionary<int, NoteGroup> NoteGroups)
    {
        this.Notes = Notes;
        this.NoteGroups = NoteGroups;

        InitKoreographyEvent();
        SetupNotes();
    }

    void InitKoreographyEvent()
    {
        koreography = Config.Koreography;

        InstantiateEventTrack = ScriptableObject.CreateInstance<KoreographyTrack>();
        Assert.IsNotNull(InstantiateEventTrack, $"Cannot create event track");
        InstantiateEventTrack.EventID = $"instantiateEventTrack-{TrackId}";
        var res = koreography.AddTrack(InstantiateEventTrack);
        Assert.IsTrue(res);

        MissEventTrack = ScriptableObject.CreateInstance<KoreographyTrack>();
        Assert.IsNotNull(MissEventTrack, $"Cannot create event track");
        MissEventTrack.EventID = $"missEventTrack-{TrackId}";
        res = koreography.AddTrack(MissEventTrack);
        Assert.IsTrue(res);

        Koreographer.Instance.RegisterForEvents(InstantiateEventTrack.EventID, InstanciateNote);
        Koreographer.Instance.RegisterForEvents(MissEventTrack.EventID, HandleNoteMiss);

        if (AutoPlay)
        {
            AutoPlayEventTrack = ScriptableObject.CreateInstance<KoreographyTrack>();
            Assert.IsNotNull(AutoPlayEventTrack, $"Cannot create event track");
            AutoPlayEventTrack.EventID = $"autoPlayEventTrack-{TrackId}";
            res = koreography.AddTrack(AutoPlayEventTrack);
            Assert.IsTrue(res);

            Koreographer.Instance.RegisterForEvents(AutoPlayEventTrack.EventID, HandleAutoPlay);
        }
    }

    void SetupNotes()
    {
        foreach (var idx in Enumerable.Range(0, Notes.Count))
        {
            var info = Notes[idx].Info;

            var insEvt = new KoreographyEvent();
            insEvt.StartSample = info.AppearedAtSample;
            insEvt.EndSample = insEvt.StartSample;
            insEvt.Payload = new IntPayload { IntVal = idx };
            InstantiateEventTrack.AddEvent(insEvt);

            if (info.NoteType == NoteType.Pseudo)
                continue;

            var missEvt = new KoreographyEvent();
            missEvt.StartSample = info.ShouldHitAtSample + Config.MaxDelayHitSample;
            missEvt.EndSample = missEvt.StartSample;
            missEvt.Payload = new IntPayload { IntVal = idx };
            MissEventTrack.AddEvent(missEvt);

            if (AutoPlay)
            {
                var autoPlayEvt = new KoreographyEvent();
                int offset;
                if (info.NoteType == NoteType.Single)
                    offset = (int)UnityEngine.Random.Range(-30.0f, 30.0f);
                else
                    offset = 10;
                autoPlayEvt.StartSample = info.ShouldHitAtSample + Config.MsToSample(offset);
                autoPlayEvt.EndSample = autoPlayEvt.StartSample;
                autoPlayEvt.Payload = new IntPayload { IntVal = idx };
                AutoPlayEventTrack.AddEvent(autoPlayEvt);
            }
        }
        noteInTrack = new NoteInTrack(Notes);
    }

    public void CleanUp()
    {
        Koreographer.Instance?.UnregisterForAllEvents(this);
        if (InstantiateEventTrack != null)
        {
            koreography.RemoveTrack(InstantiateEventTrack);
            InstantiateEventTrack = null;
        }
        if (MissEventTrack != null)
        {
            koreography.RemoveTrack(MissEventTrack);
            MissEventTrack = null;
        }
        if (AutoPlayEventTrack != null)
        {
            koreography.RemoveTrack(AutoPlayEventTrack);
            AutoPlayEventTrack = null;
        }
        koreography = null;

        Notes?.ForEach(note => note.Dispose());
        Notes = null;
        NoteGroups = null;
        noteInTrack = null;
        noteLink.CleanUp();
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
        CleanUp();
    }

    /// <summary>
    /// 根据事件中所带的 noteID 在游戏中生成对应的 <c>Note</c> 和 <c>NoteLink</c>
    /// </summary>
    void InstanciateNote(KoreographyEvent evt)
    {
        var note = Notes[evt.GetIntValue()];
        if (note.Instantiated)
            return;
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
            break;
        }

        noteLink.Add(note);
    }

    public void HandleNoteMiss(KoreographyEvent evt)
    {
        var note = Notes[evt.GetIntValue()];
        if (note.Verdict == null)
            note.Verdict = NoteVerdict.Miss;
    }

    public void HandleAutoPlay(KoreographyEvent evt)
    {
        Click(true);
    }

    public bool Click(bool isBegining)
    {
        if (koreography == null)
            return false;

        var currentSample = Config.CurrentSample;
        var noteAndOffset = noteInTrack.GetCurrentNoteAndSample(currentSample);
        if (noteAndOffset == null)
            return false;
        var (note, offset) = ((Note, int))noteAndOffset;

        switch (note.Info.NoteType)
        {
        case NoteType.Single:
            if (isBegining)
            {
                note.Verdict = NoteVerdict.FromSample(offset);
            }
            return true;
        case NoteType.Hold:
            if (offset >= 0)
            {
                note.Verdict = NoteVerdict.FromSample(offset);
                return true;
            }
            return false;
        }
        return false;
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
                return (note, offset);
            }
            // Too early, ignored
            return null;
        }
        return null;
    }

    private SortedDictionary<int, Note> notes = new SortedDictionary<int, Note>();
}
