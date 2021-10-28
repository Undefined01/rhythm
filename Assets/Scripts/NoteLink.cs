using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using SonicBloom.Koreo;

public class NoteLink : MonoBehaviour
{
    public Koreography koreography;

    static readonly Vector3 TrackStartPos = new Vector3(0, 0, 10);

    List<Note> notes = new List<Note>();
    LineRenderer line;
    public Vector3 EndPoint { get; private set; }

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;
        line.SetPositions(new Vector3[] {});
        FixedUpdate();
    }

    void FixedUpdate()
    {
        if (notes.Count == 0)
            return;

        var currentSample = koreography.GetLatestSampleTime();
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

    float? CalcLinkSphereIntersection(Vector3 p0, Vector3 dp, float r)
    {
        var a = dp.sqrMagnitude;
        var b = 2 * Vector3.Dot(p0, dp);
        var c = p0.sqrMagnitude - r * r;
        var delta = b * b - 4 * a * c;
        if (delta < 0)
            return null;
        var root = (-b + Mathf.Sqrt(delta)) / (2 * a);
        if (root > 0)
            return root;
        else
            return null;
    }
}
