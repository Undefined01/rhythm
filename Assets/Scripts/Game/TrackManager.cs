using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using UnityEngine;
using UnityEngine.Assertions;

using SonicBloom.Koreo;
using SonicBloom.Koreo.Players;

public class TrackManager : MonoBehaviour
{
    public int StartAtSample = 0;

    public Camera Camera;
    public List<GameObject> TrackObjects;
    public List<Koreography> Koreographies;

    protected List<Track> Tracks;
    protected List<List<Note>> notes;
    protected Dictionary<int, NoteGroup> noteGroups;
    protected SimpleMusicPlayer simplePlayer;
    protected KoreographyTrack finishedEventTrack;

    public int ExpectedHitsInTotal;
    public VerdictStatistics Statistics;
    public event Action<object, NoteVerdict> OnHandleVerdict;

    public event Action OnFinished;

    public void CleanUp()
    {
        Tracks?.ForEach(x => x.CleanUp());
        if (finishedEventTrack != null)
        {
            Config.Koreography.RemoveTrack(finishedEventTrack);
            Koreographer.Instance?.UnregisterForAllEvents(this);
            finishedEventTrack = null;
        }
        simplePlayer?.Stop();

        notes = null;
        noteGroups = null;
        Statistics = new VerdictStatistics();
    }

    void OnDestroy()
    {
        CleanUp();
    }

    void Awake()
    {
        var koreographer = GameObject.Find("Koreographer");
        simplePlayer = koreographer.GetComponent<SimpleMusicPlayer>();
        Assert.IsNotNull(simplePlayer);

        Assert.IsTrue(TrackObjects.All(t => t != null));

        // Create an empty track for registering runtime event
        Tracks = TrackObjects
                     .Select((trackObject, idx) => {
                         var track = trackObject.GetComponent<Track>();
                         Assert.IsNotNull(track);
                         track.TrackId = idx + 1;
                         return track;
                     })
                     .ToList();
    }

    public void StartLevel(string level)
    {
        var noteInfoAsset = (TextAsset)Resources.Load($"NoteChart/{level}");
        List<NoteInfo> noteInfos;
        using (var reader = new System.IO.MemoryStream(noteInfoAsset.bytes))
        {
            XmlSerializer xz = new XmlSerializer(typeof(List<NoteInfo>));
            noteInfos = (List<NoteInfo>)xz.Deserialize(reader);
        }
        StartLevelWithNote(level, noteInfos);
    }

    public void StartLevelWithNote(string level, List<NoteInfo> noteInfos)
    {
        CleanUp();

        // Get rhythm track
        var koreography = Koreographies.Single(k => k.name == level);
        Config.Set(koreography);

        // Add FinishedEventTrack
        finishedEventTrack = ScriptableObject.CreateInstance<KoreographyTrack>();
        finishedEventTrack.EventID = $"finishedEventTrack";
        koreography.AddTrack(finishedEventTrack);
        var finishedEvent = new KoreographyEvent();
        finishedEvent.StartSample = koreography.SourceClip.samples - koreography.SampleRate / 4;
        finishedEvent.EndSample = finishedEvent.StartSample;
        finishedEventTrack.AddEvent(finishedEvent);
        Koreographer.Instance.RegisterForEvents("finishedEventTrack", FinishedEventCallback);

        // Read noteInfos
        notes = Enumerable.Range(0, 4).Select(_ => new List<Note>()).ToList();
        noteGroups = new Dictionary<int, NoteGroup>();
        ExpectedHitsInTotal = 0;
        foreach (var info in noteInfos)
        {
            info.AppearedAtSample += Config.OffsetSample;
            info.ShouldHitAtSample += Config.OffsetSample;
            var note = new Note(info);
            if (info.Track <= 0 || info.Track > Tracks.Count)
            {
                Debug.LogWarning($"轨道{info.Track}不存在（在{info.ShouldHitAtSample}处）");
                continue;
            }
            notes[info.Track - 1].Add(note);
            if (info.Group != 0)
            {
                if (!noteGroups.ContainsKey(info.Group))
                {
                    ExpectedHitsInTotal++;
                    noteGroups[info.Group] = new NoteGroup();
                    noteGroups[info.Group].OnHasVerdict += HandleVerdict;
                }
                noteGroups[info.Group].Add(note);
            }
            else
            {
                ExpectedHitsInTotal++;
                note.OnHasVerdict += HandleVerdict;
            }
        }

        foreach (var i in Enumerable.Range(0, Tracks.Count))
        {
            Tracks[i].Init(notes[i], noteGroups);
        }

        Debug.Log($"Start {level}");
        simplePlayer.gameObject.GetComponent<AudioSource>().volume = SaveManager.Save.Settings.MusicVolumn;
        simplePlayer.LoadSong(koreography, StartAtSample);
    }

    void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        foreach (var touch in Input.touches)
        {
            HandleTouch(touch.position, touch.phase == TouchPhase.Began);
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch(Input.mousePosition, true);
        }
        else if (Input.GetMouseButton(0))
        {
            HandleTouch(Input.mousePosition, false);
        }
#endif
    }

    void HandleTouch(Vector3 pos, bool isBegining)
    {
        var dis = Tracks.Select(t => (Vector3.Distance(pos, Camera.main.WorldToScreenPoint(t.TrackEndPoint)), t))
                      .OrderBy(x => x.Item1);
        foreach (var (d, track) in dis)
        {
            if (d > Camera.pixelHeight / 5)
                break;
            if (track.Click(isBegining))
                break;
        }
    }

    protected void HandleVerdict(object sender, NoteVerdict verdict)
    {
        Debug.Log($"Verdict #{verdict.Grade} {verdict.OffsetMs} ms");
        Statistics.Add(verdict);
        OnHandleVerdict?.Invoke(sender, verdict);
    }

    void FinishedEventCallback(KoreographyEvent evt)
    {
        OnFinished?.Invoke();
    }
}
