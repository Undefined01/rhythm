using System;
using UnityEngine;
using UnityEngine.Assertions;

using SonicBloom.Koreo;

public class NoteBehaviour : MonoBehaviour
{
    public NoteInfo Info;

    public GameObject HitEffect;

    void Start()
    {
        transform.position = Config.NoteStartPos;
    }

    void FixedUpdate()
    {
        var currentSample = Config.CurrentSample;
        transform.position = Info.CalcPosition(currentSample);
    }

    public event Action OnDestroyEvent;
    void OnDestroy()
    {
        OnDestroyEvent?.Invoke();
    }

    public void SetVerdict(NoteVerdict verdict)
    {
        Destroy(this.gameObject);
        if (HitEffect != null && verdict.Grade <= NoteGrade.Good)
            GameObject.Instantiate(HitEffect, this.transform.position, Quaternion.identity);
    }
}
