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

    protected List<Track> Tracks;
    protected List<List<Note>> notes;
    protected Dictionary<int, NoteGroup> noteGroups;
    protected Koreography koreography;
    protected SimpleMusicPlayer simplePlayer;

    public VerdictStatistics Statistics;

    public void CleanUp()
    {
        Tracks?.ForEach(x => x.CleanUp());
        simplePlayer.Stop();

        notes = null;
        noteGroups = null;
        Statistics = new VerdictStatistics();
    }

    void Awake()
    {
        simplePlayer = GameObject.Find("Koreographer")?.GetComponent<SimpleMusicPlayer>();
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

    public void StartLevel(int LevelID)
    {
        CleanUp();

        // Get rhythm track
        koreography = Koreographer.Instance.GetKoreographyAtIndex(LevelID);
        Assert.IsNotNull(koreography, $"Cannot find koreography {LevelID}");

        Config.Set(koreography);

        // Read noteInfos
        notes = Enumerable.Range(0, 4).Select(_ => new List<Note>()).ToList();
        noteGroups = new Dictionary<int, NoteGroup>();

        Debug.Log(koreography.name);
        var noteInfoAsset = (TextAsset)Resources.Load(koreography.name);
        var noteInfoStr = System.Text.Encoding.ASCII.GetBytes(noteInfoAsset.text);
        using (var reader = new System.IO.MemoryStream(noteInfoStr))
        {
            XmlSerializer xz = new XmlSerializer(typeof(List<NoteInfo>));
            var noteInfos = (List<NoteInfo>)xz.Deserialize(reader);
            foreach (var info in noteInfos)
            {
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
                        noteGroups[info.Group] = new NoteGroup();
                    noteGroups[info.Group].Add(note);
                }
            }
        }
        foreach (var i in Enumerable.Range(0, Tracks.Count))
        {
            Tracks[i].Init(notes[i], noteGroups);
        }

        simplePlayer.LoadSong(koreography, StartAtSample);
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
            {
                break;
            }
    }

    protected virtual void HandleVerdict(object sender, NoteVerdict verdict)
    {
        Statistics.Add(verdict);
    }
}
