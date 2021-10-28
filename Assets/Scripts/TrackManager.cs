using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

using SonicBloom.Koreo;

public class TrackManager : MonoBehaviour
{
    public int AudioID = 0;
    public string NoteFile = "piano";
    public Text FeedBackText;

    protected const int NumOfTrack = 4;
    public List<GameObject> TrackObjects;
    protected List<Track> Tracks;
    protected Dictionary<int, NoteGroup> noteGroups = new Dictionary<int, NoteGroup>();
    protected Koreography koreography;

    int maxCombo = 0;
    int _combo = 0;
    int combo
    {
        get => _combo;
        set {
            _combo = value;
            maxCombo = maxCombo > _combo? maxCombo : _combo;
        }
    }
    int _accuracyN = 0;
    int _accuracyD = 0;
    float accuracy => _accuracyD == 0 ? 1 : (float)_accuracyN / _accuracyD;

    // int accuracyTot = 0;

    void Awake()
    {
        // Get rhythm track
        koreography = Koreographer.Instance.GetKoreographyAtIndex(AudioID);
        Assert.IsNotNull(koreography, $"Cannot find koreography {AudioID}");
        
        Config.Set(koreography);

        // Assert.AreEqual(Tracks.Count, NumOfTrack);
        Assert.IsTrue(TrackObjects.All(t => t != null));

        // Create an empty track for registering runtime event
        Tracks = TrackObjects
                     .Select(to => {
                         var track = to.GetComponent<Track>();
                         Assert.IsNotNull(track, $"Failed to get `Track` Component of {to}");

                         var insEventTrack = ScriptableObject.CreateInstance<KoreographyTrack>();
                         Assert.IsNotNull(insEventTrack, $"Cannot create event track");
                         insEventTrack.EventID = $"instantiateEventTrack-{track.TrackId}";
                         koreography.AddTrack(insEventTrack);

                         var missEventTrack = ScriptableObject.CreateInstance<KoreographyTrack>();
                         Assert.IsNotNull(missEventTrack, $"Cannot create event track");
                         missEventTrack.EventID = $"missEventTrack-{track.TrackId}";
                         koreography.AddTrack(missEventTrack);

                         track.koreography = koreography;
                         track.InstantiateEventTrack = insEventTrack;
                         track.MissEventTrack = missEventTrack;
                         track.Notes = new List<Note>();
                         track.NoteGroups = noteGroups;
                         track.HandleVerdict = HandleVerdict;
                         return track;
                     })
                     .ToList();

        // Read noteInfos
        var noteInfoAsset = (TextAsset)Resources.Load(NoteFile);
        var noteInfoStr = System.Text.Encoding.ASCII.GetBytes(noteInfoAsset.text);
        using (var reader = new System.IO.MemoryStream(noteInfoStr))
        {
            XmlSerializer xz = new XmlSerializer(typeof(List<NoteInfo>));
            var noteInfos = (List<NoteInfo>)xz.Deserialize(reader);
            foreach (var info in noteInfos)
            {
                try
                {
                    AddNoteInfo(info);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Debug.LogError(ex);
                }
            }
        }
    }

    void Update()
    {
        foreach (var touch in Input.touches)
        {
            HandleTouch(touch.position, touch.phase == TouchPhase.Began);
        }
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch(Input.mousePosition, true);
        }
        else if (Input.GetMouseButton(0))
        {
            HandleTouch(Input.mousePosition, false);
        }
    }

    void HandleTouch(Vector3 pos, bool isBegining)
    {
        var dis = Tracks.Select(t => (Vector3.Distance(pos, Camera.main.WorldToScreenPoint(t.TrackEndPoint)), t))
                      .OrderBy(x => x.Item1);
        foreach (var (_, track) in dis)
            if (track.Click(isBegining))
                break;
    }

    protected virtual void AddNoteInfo(NoteInfo info)
    {
        var note = new Note(koreography, info);
        if (info.Track <= 0 || info.Track > Tracks.Count)
            throw new ArgumentOutOfRangeException($"轨道{info.Track}不存在（在{info.ShouldHitAtSample}处）");
        var idx = Tracks[info.Track - 1].Notes.Count;

        var insEvt = new KoreographyEvent();
        insEvt.StartSample = info.AppearedAtSample;
        insEvt.EndSample = insEvt.StartSample;
        insEvt.Payload = new IntPayload { IntVal = idx };
        Tracks[info.Track - 1].InstantiateEventTrack.AddEvent(insEvt);

        var missEvt = new KoreographyEvent();
        missEvt.StartSample = info.ShouldHitAtSample + Config.MaxDelayHitSample;
        missEvt.EndSample = missEvt.StartSample;
        missEvt.Payload = new IntPayload { IntVal = idx };
        Tracks[info.Track - 1].MissEventTrack.AddEvent(missEvt);

        Tracks[info.Track - 1].Notes.Add(note);
        if (info.Group != 0)
        {
            if (!noteGroups.ContainsKey(info.Group))
                noteGroups[info.Group] = new NoteGroup();
            var group = noteGroups[info.Group];
            group.Add(note);
        }
    }

    protected virtual void HandleVerdict(object sender, NoteVerdict verdict)
    {
        var grade = verdict.Grade;
        switch (grade)
        {
        case NoteGrade.Perfect:
            combo++;
            AddAccuracy(100);
            break;
        case NoteGrade.Good:
            combo++;
            AddAccuracy(65);
            break;
        case NoteGrade.Bad:
            combo = 0;
            AddAccuracy(25);
            break;
        case NoteGrade.Miss:
            combo = 0;
            AddAccuracy(0);
            break;
        }
        FeedBackText.text = $"{grade} ({verdict.OffsetMs}ms)\nCombo {combo}\nAccuracy {accuracy}";
    }

    void AddAccuracy(int percent)
    {
        _accuracyN += percent;
        _accuracyD += 100;
    }
}
