using System;
using UnityEngine;
using UnityEngine.Assertions;

using SonicBloom.Koreo;

public class Note
{
    Koreography koreography;
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
                NoteBehaviour?.SetVerdict(verdict);
                OnHasVerdict?.Invoke(this, verdict);
            }
        }
    }
    public event Action<object, NoteVerdict> OnHasVerdict;

    public Note(Koreography koreography, NoteInfo info)
    {
        this.koreography = koreography;
        this.Info = info;
    }

    public void SetNoteObject(GameObject obj)
    {
        NoteBehaviour = obj.GetComponent<NoteBehaviour>();
        Assert.IsNotNull(NoteBehaviour);
        NoteBehaviour.koreography = koreography;
        NoteBehaviour.Info = Info;
        NoteBehaviour.OnDestroyEvent += () => NoteBehaviour = null;
    }
}
