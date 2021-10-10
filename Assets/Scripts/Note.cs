using UnityEngine;

using SonicBloom.Koreo;

public class Note : MonoBehaviour
{
    public Koreography koreography;
    public NoteInfo Info;

    void Start()
    {
        transform.position = Info.AppearedAtPos;
    }

    void FixedUpdate()
    {
        var currentSample = koreography.GetLatestSampleTime();
        transform.position = Info.CalcPosition(currentSample);
    }

    public void Hit()
    {
        Destroy(this.gameObject);
    }

    public void Miss()
    {
        Destroy(this.gameObject);
    }
}
