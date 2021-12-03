using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using SonicBloom.Koreo;

public class NoteLink : MonoBehaviour
{
    static readonly Vector3 TrackStartPos = Config.NoteStartPos;

    List<Note> notes = new List<Note>();
    LineRenderer line;
    public Vector3 EndPoint { get; private set; }

    void Start()
    {
        line = GetComponent<LineRenderer>();
        CleanUp();
        FixedUpdate();
    }

    void FixedUpdate()
    {
        if (notes.Count == 0)
            return;

        var currentSample = Config.CurrentSample;
        var pos = notes.Select(x => x.Info.CalcPosition(currentSample)).ToList();
        while (pos.Count >= 2 && pos[1].z <= 0)
            pos.RemoveAt(0);
        pos.Add(TrackStartPos);
        if (pos.Count >= 2 && pos[0].z < 0)
        {
            var pos1 = pos[0];
            var pos2 = pos[1];
            pos[0] = Vector3.Lerp(pos2, pos1, pos2.z / (pos2.z - pos1.z));
        }
        EndPoint = pos[0];
        line.positionCount = pos.Count;
        line.SetPositions(pos.ToArray());
    }

    public void Add(Note note)
    {
        notes.Add(note);
    }

    public void CleanUp()
    {
        notes.Clear();
        if (line != null)
        {
            line.positionCount = 0;
            line.SetPositions(new Vector3[] {});
        }
    }

    void OnDestroy()
    {
        line = null;
    }
}
