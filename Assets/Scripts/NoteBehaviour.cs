using System;
using UnityEngine;
using UnityEngine.Assertions;

using SonicBloom.Koreo;

public class NoteBehaviour : MonoBehaviour
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

    public event Action OnDestroyEvent;
    void OnDestroy()
    {
        OnDestroyEvent?.Invoke();
    }

    public void SetVerdict(NoteVerdict verdict)
    {
        Destroy(this.gameObject);
    }
}
