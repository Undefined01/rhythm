using System;
using UnityEngine;
using UnityEngine.Assertions;

using SonicBloom.Koreo;

public class Note : IDisposable
{
    public NoteInfo Info;
    public NoteBehaviour NoteBehaviour = null;

    public bool Instantiated = false;

    private NoteVerdict verdict;
    public NoteVerdict Verdict
    {
        get {
            return verdict;
        }
        set {
            if (verdict == null)
            {
                verdict = value;
                if (NoteBehaviour == null)
                    Debug.LogWarning($"Verdicting Note #{Info.ShouldHitAtSample} without instance");
                NoteBehaviour?.SetVerdict(verdict);
                OnHasVerdict?.Invoke(this, verdict);
            }
            else
            {
                Debug.LogWarning(
                    $"Double assignment of Note #{Info.ShouldHitAtSample}.\nOriginal: {verdict.OffsetMs}, Try set: {value.OffsetMs}");
            }
        }
    }
    public event Action<object, NoteVerdict> OnHasVerdict;

    public Note(NoteInfo info)
    {
        this.Info = info;
    }

    public void SetNoteObject(GameObject obj)
    {
        NoteBehaviour = obj.GetComponent<NoteBehaviour>();
        Assert.IsNotNull(NoteBehaviour);
        NoteBehaviour.Info = Info;
        NoteBehaviour.OnDestroyEvent += () => NoteBehaviour = null;
    }

    public void Dispose()
    {
        if (NoteBehaviour != null)
            GameObject.Destroy(NoteBehaviour.gameObject);
        NoteBehaviour = null;
    }
}
