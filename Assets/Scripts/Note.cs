using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SonicBloom.Koreo;

public class Note : MonoBehaviour
{
    public Koreography koreography;
    public NoteInfo Info;

    bool distroyed = false;
    float duration;
    Vector3 targetPos;

    void Start()
    {
        transform.position = Info.AppearedAtPos;
        targetPos = Info.AppearedAtPos + 2 * (Info.ShouldHitAtPos - Info.AppearedAtPos);
        duration = (float)(Info.ShouldHitAtSample - Info.AppearedAtSample) * 2;
    }

    void FixedUpdate()
    {
        var currentSample = koreography.GetLatestSampleTime();
        var t = (float)(currentSample - Info.AppearedAtSample) / duration;
        transform.position = Vector3.Lerp(Info.AppearedAtPos, targetPos, t);
    }

    public void Hit()
    {
        if (!distroyed)
        {
            Destroy(this.gameObject);
            distroyed = true;
        }
    }

    public void Miss()
    {
        if (!distroyed)
        {
            Destroy(this.gameObject);
            distroyed = true;
        }
    }
}
