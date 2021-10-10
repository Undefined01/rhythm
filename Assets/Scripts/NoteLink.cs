using UnityEngine;

using SonicBloom.Koreo;

public class NoteLink : MonoBehaviour
{
    public Koreography koreography;
    public NoteInfo node1, node2;

    static Vector3 TrackStartPos = new Vector3(0, 0, 10);
    static float TrackLen = 10;

    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        FixedUpdate();
    }

    void FixedUpdate()
    {
        var currentSample = koreography.GetLatestSampleTime();
        var pos1 = node1.CalcPosition(currentSample);
        var pos2 = node2.CalcPosition(currentSample);
        var t = CalcLinkSphereIntersection(pos2 - TrackStartPos, pos1 - pos2, TrackLen) ?? -1;
        if ((pos2 - TrackStartPos).magnitude > TrackLen)
        {
            Destroy(this.gameObject);
            return;
        }
        if (t < 1)
        {
            pos1 = pos2 + (pos1 - pos2) * t;
        }
        line.SetPositions(new Vector3[] { pos2, pos1 });
    }

    public delegate void UpdateNoteLinkNode(NoteInfo node2);
    public void UpdateNode2(NoteInfo node2)
    {
        this.node2 = node2;
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
