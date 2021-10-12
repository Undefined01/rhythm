using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
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
    public int AudioID;
    public string EventID = "noteTrack";
    public KeyCode Key;
    public GameObject SingleNoteObject, HoldNoteObject;
    public GameObject NoteLink;

    public Vector3 startPos, endPos;

    public Text FeedBackText;

    private Koreography koreography;
    private KoreographyTrack rhythmTrack, eventTrack;

    private List<Note> notes;
    private Dictionary<int, NoteGroup> noteGroups = new Dictionary<int, NoteGroup>();

    private TimeSpan MaxTolerantTime = TimeSpan.FromMilliseconds(500);
    private NoteInTrack noteInTrack;

    private NoteLink.UpdateNoteLinkNode updateNote2OfLastNoteLink;

    void Start()
    {
        // Get rhythm track
        koreography = Koreographer.Instance.GetKoreographyAtIndex(AudioID);
        Assert.IsNotNull(koreography, $"Cannot find koreography {AudioID}");
        rhythmTrack = koreography.GetTrackByID(EventID);
        Assert.IsNotNull(rhythmTrack, $"Cannot find rhythm track {EventID}");

        // Create an empty track for registering runtime event
        eventTrack = ScriptableObject.CreateInstance<KoreographyTrack>();
        Assert.IsNotNull(rhythmTrack, $"Cannot create event track");
        eventTrack.EventID = $"runtimeEventTrack-{EventID}";
        koreography.AddTrack(eventTrack);

        // Initialize parameters
        var MaxToleranceSample = (int)(MaxTolerantTime.TotalSeconds * koreography.SampleRate);
        noteInTrack = new NoteInTrack(MaxToleranceSample);

        // Read noteInfos
        using (var reader = System.IO.File.OpenRead($"track{EventID}.xml"))
        {
            XmlSerializer xz = new XmlSerializer(typeof(List<NoteInfo>));
            var noteInfos = (List<NoteInfo>)xz.Deserialize(reader);
            notes = noteInfos
                        .Select((info, idx) => {
                            var genEvt = new KoreographyEvent();
                            var interval = 1.0;
                            var advance = (int)(koreography.SampleRate * interval);
                            genEvt.StartSample = info.AppearedAtSample;
                            genEvt.EndSample = genEvt.StartSample;
                            genEvt.Payload = new IntPayload { IntVal = idx };
                            eventTrack.AddEvent(genEvt);

    var note = new Note(koreography, info);

                            if (info.Group != 0)
                            {
                                if (!noteGroups.ContainsKey(info.Group))
                                    noteGroups[info.Group] = new NoteGroup();
                                var group = noteGroups[info.Group];
                                group.Add(note);
                            }

                            return note;
                        })
                        .ToList();
        }

        Koreographer.Instance.RegisterForEvents(eventTrack.EventID, InstanciateNote);
    }

    void Update()
    {
        var currentSample = koreography.GetLatestSampleTime();
        var noteAndOffset = noteInTrack.GetCurrentNoteAndSample(currentSample);
        if (noteAndOffset == null)
            return;
        var (note, offset) = ((Note, int))noteAndOffset;

        switch (note.Info.NoteType)
        {
        case NoteType.Single:
            if (Input.GetKeyDown(Key))
            {
                noteInTrack.Remove(note);
                note.Verdict = JudgeNote(offset);
            }
            break;
        case NoteType.Hold:
            if (offset >= 0 && Input.GetKey(Key))
            {
                noteInTrack.Remove(note);
                note.Verdict = JudgeNote(offset);
            }
            break;
        }
    }

    void OnDestroy()
    {
        koreography?.RemoveTrack(eventTrack);
        Koreographer.Instance?.UnregisterForAllEvents(this);
    }

    /// <summary>
    /// 根据事件中所带的 noteID 在游戏中生成对应的 <c>Note</c> 和 <c>NoteLink</c>
    /// </summary>
    void InstanciateNote(KoreographyEvent evt)
    {
        var note = notes[evt.GetIntValue()];
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
            return;
        }
        noteInTrack.Add(note);

        if (note.Info.Group != 0)
        {
            var group = noteGroups[note.Info.Group];
            note.OnHasVerdict += group.UpdateVerdict;
            group.OnHasVerdict += HandleVerdict;
        }
        else
        {
            note.OnHasVerdict += HandleVerdict;
        }

        if (updateNote2OfLastNoteLink != null)
            updateNote2OfLastNoteLink(note.Info);
        InstanciateNoteLink(note.Info);
    }

    void InstanciateNoteLink(NoteInfo node1)
    {
        var noteLinkObject = GameObject.Instantiate(NoteLink);
        var noteLink = noteLinkObject.GetComponent<NoteLink>();
        Assert.IsNotNull(noteLink);
        noteLink.koreography = koreography;
        noteLink.node1 = node1;
        noteLink.node2 = new NoteInfo {
            AppearedAtPos = startPos,
            ShouldHitAtPos = startPos,
            AppearedAtSample = 0,
            ShouldHitAtSample = 10000000,
        };
        updateNote2OfLastNoteLink = noteLink.UpdateNode2;
    }

    void HandleVerdict(object sender, NoteVerdict verdict)
    {
        FeedBackText.text = $"{verdict.Grade} ({verdict.OffsetMs}ms)";
    }

    NoteVerdict JudgeNote(int offsetSample)
    {
        var offsetMs = offsetSample * 1000 / koreography.SampleRate;
        return new NoteVerdict(offsetMs);
    }
}

public class NoteInTrack
{
    public NoteInTrack(int MaxTolerantSample)
    {
        this.MaxTolerantSample = MaxTolerantSample;
    }

    public void Add(Note note)
    {
        notes.Add(note.Info.ShouldHitAtSample, note);
    }
    public void Remove(Note note)
    {
        notes.Remove(note.Info.ShouldHitAtSample);
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
            if (offset > MaxTolerantSample)
            {
                // Too late, raise `MissEvent`
                note.Verdict = new NoteVerdict(505);
                notes.Remove(hitAtSample);
            }
            else
            {
                if (offset > -MaxTolerantSample)
                {
                    return (note, offset);
                }
                // Too early, ignored
                return null;
            }
        }
        return null;
    }

    private int MaxTolerantSample;
    private SortedDictionary<int, Note> notes = new SortedDictionary<int, Note>();
}
