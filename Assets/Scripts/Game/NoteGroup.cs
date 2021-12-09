using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;

public class NoteGroup
{
    private List<Note> notes = new List<Note>();
    private int hitCount = 0;
    private bool end;

    public void Add(Note note)
    {
        notes.Add(note);
        note.OnHasVerdict += UpdateVerdict;
    }

    public void UpdateVerdict(object sender, NoteVerdict verdict)
    {
        if (end)
            return;

        if (verdict.Grade == NoteGrade.Miss)
        {
            end = true;
            notes.Where(note => note.Verdict != null).Select(note => note.Verdict = verdict);
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
