using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

using SonicBloom.Koreo;

public class Track : MonoBehaviour
{
    public int audioID;
    public string eventID = "noteTrack";
    public GameObject SingleNote;

    private Koreography koreography;
    private KoreographyTrack rhythmTrack, eventTrack;

    void Start()
    {
        // Get rhythm track
        koreography = Koreographer.Instance.GetKoreographyAtIndex(audioID);
        Assert.IsNotNull(koreography, $"Cannot find koreography {audioID}");
        rhythmTrack = koreography.GetTrackByID(eventID);
        Assert.IsNotNull(rhythmTrack, $"Cannot find rhythm track {eventID}");

        // Create an empty track for registering runtime event
        eventTrack = ScriptableObject.CreateInstance<KoreographyTrack>();
        Assert.IsNotNull(rhythmTrack, $"Cannot create event track");
        eventTrack.EventID = "runtimeEventTrack";
        koreography.AddTrack(eventTrack);

        // Generate temporary "GenerateNote" event
        var allEvents = rhythmTrack.GetAllEvents();
        foreach (var evt in allEvents)
        {
            var genEvt = new KoreographyEvent();
            var interval = 1.0;
            var advance = (int)(koreography.SampleRate * interval);
            genEvt.StartSample = evt.StartSample - advance;
            genEvt.EndSample = evt.EndSample - advance;
            genEvt.Payload = evt.Payload;
            eventTrack.AddEvent(genEvt);
        }

        Koreographer.Instance.RegisterForEventsWithTime(eventTrack.EventID, GenerateNote);
    }

    void OnDestroy()
    {
        koreography?.RemoveTrack(eventTrack);
        Koreographer.Instance?.UnregisterForAllEvents(this);
    }

    void GenerateNote(KoreographyEvent evt, int sampleTime, int sampleDelta, DeltaSlice deltaSlice)
    {
        var note = GameObject.Instantiate(SingleNote);
        note.transform.position = new Vector3(0, 0, 10);
    }
}
