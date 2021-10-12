using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

class NoteGroup
{
    private List<Note> notes = new List<Note>();
    private int hitCount = 0;
    private bool end;

    public void Add(Note info)
    {
        notes.Add(info);
    }

    public void UpdateVerdict(object sender, NoteVerdict verdict)
    {
        if (end) return;

        if (verdict.Grade == NoteGrade.Miss)
        {
            end = true;
            notes.ForEach(note => note.Verdict = verdict);
            OnHasVerdict?.Invoke(this, verdict);
            return;
        }
        
        hitCount++;
        if (hitCount == notes.Count)
        {
            end = true;
            OnHasVerdict?.Invoke(this, verdict);
            return;
        }
    }

    public event Action<object, NoteVerdict> OnHasVerdict;
}
