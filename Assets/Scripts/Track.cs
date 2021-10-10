using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

using SonicBloom.Koreo;

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

    private List<NoteInfo> noteInfos = new List<NoteInfo>();
    private HashSet<(int, int)> noteEmited = new HashSet<(int, int)>();

    private TimeSpan MaxToleranceTime = TimeSpan.FromMilliseconds(500);
    private NoteInTrack singleNote;
    private NoteInTrack holdNote;

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
        var MaxToleranceSample = (int)(MaxToleranceTime.TotalSeconds * koreography.SampleRate);
        singleNote = new NoteInTrack(MaxToleranceSample);
        holdNote = new NoteInTrack(MaxToleranceSample);
        singleNote.MissEvent += (object sender, Note note, int offset) => Judge(offset);
        holdNote.MissEvent += (object sender, Note note, int offset) => Judge(offset);

        // Initialize NoteLinks

        // Create temporary "InstanciateNote" event
        rhythmTrack.EnsureEventOrder();
        var allEvents = rhythmTrack.GetAllEvents();
        foreach (var evt in allEvents)
        {
            var genEvt = new KoreographyEvent();
            var interval = 1.0;
            var advance = (int)(koreography.SampleRate * interval);
            genEvt.StartSample = evt.StartSample - advance;
            genEvt.EndSample = genEvt.StartSample;
            genEvt.Payload = new IntPayload { IntVal = noteInfos.Count };
            var rnd = UnityEngine.Random.insideUnitSphere * 0.3f;
            noteInfos.Add(new NoteInfo {
                Track = 1,
                NoteType = NoteType.Single,
                NoteStyle = NoteStyle.Normal,
                Group = 0,
                AppearedAtPos = startPos,
                ShouldHitAtPos = endPos + rnd,
                AppearedAtSample = genEvt.StartSample,
                ShouldHitAtSample = evt.EndSample,
            });
            eventTrack.AddEvent(genEvt);
        }

        // using (var writer = System.IO.File.CreateText("track1.xml"))
        // {
        //     XmlSerializer xz = new XmlSerializer(noteInfos.GetType());
        //     xz.Serialize(writer, noteInfos);
        // }
        Koreographer.Instance.RegisterForEvents(eventTrack.EventID, InstanciateNote);
    }

    void Update()
    {
        var currentSample = koreography.GetLatestSampleTime();
        {
            var noteAndOffset = singleNote.GetCurrentNoteAndSample(currentSample);
            if (noteAndOffset is(Note note, int offset) && Input.GetKeyDown(Key))
            {
                singleNote.Hit(note);
                Judge(offset);
            }
        }

        {
            var noteAndOffset = holdNote.GetCurrentNoteAndSample(currentSample);
            if (noteAndOffset is(Note note, int offset) && Input.GetKey(Key))
            {
                if (offset >= 0)
                {
                    holdNote.Hit(note);
                    Judge(offset);
                }
            }
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
        var noteInfo = noteInfos[evt.GetIntValue()];
        if (noteEmited.Contains((noteInfo.Track, noteInfo.AppearedAtSample)))
            return;
        noteEmited.Add((noteInfo.Track, noteInfo.AppearedAtSample));

        GameObject noteObject;
        Note note;
        switch (noteInfo.NoteType)
        {
        case NoteType.Single:
            noteObject = GameObject.Instantiate(SingleNoteObject);
            note = noteObject.GetComponent<Note>();
            singleNote.Add(noteInfo.ShouldHitAtSample, note);
            break;
        case NoteType.Hold:
            noteObject = GameObject.Instantiate(HoldNoteObject);
            note = noteObject.GetComponent<Note>();
            holdNote.Add(noteInfo.ShouldHitAtSample, note);
            break;
        default:
            return;
        }
        note.koreography = koreography;
        note.Info = noteInfo;

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

    void Judge(int offset)
    {
        var ms = offset * 1000 / koreography.SampleRate;
        if (ms > 500 || ms < -500)
            FeedBackText.text = $"Miss ({ms}ms)";
        else if (ms > 200 || ms < -200)
            FeedBackText.text = $"Bad ({ms}ms)";
        else if (ms > 70 || ms < -70)
            FeedBackText.text = $"Good ({ms}ms)";
        else
            FeedBackText.text = $"Perfect ({ms}ms)";
    }
}

public class NoteInTrack
{
    public NoteInTrack(int MaxTolerantSample)
    {
        this.MaxTolerantSample = MaxTolerantSample;
    }

    public void Add(int sampleTime, Note note)
    {
        notes.Add(sampleTime, note);
    }
    public void Hit(Note note)
    {
        notes.Remove(note.Info.ShouldHitAtSample);
        note.Hit();
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
                MissEvent?.Invoke(this, note, offset);
                notes.Remove(hitAtSample);
                note.Miss();
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

    public delegate void MissHandler(object sender, Note note, int offset);
    public event MissHandler MissEvent;

    private int MaxTolerantSample;
    private SortedDictionary<int, Note> notes = new SortedDictionary<int, Note>();
}
