using UnityEngine;

public enum NoteGrade
{
    Perfect,
    Good,
    Bad,
    Miss,
}

public class NoteVerdict
{
    public NoteVerdict(int offsetMs)
    {
        this.OffsetMs = offsetMs;
    }

    public int OffsetMs;
    public NoteGrade Grade
    {
        get {
            if (OffsetMs >= 500 || OffsetMs <= -500)
                return NoteGrade.Miss;
            else if (OffsetMs >= 200 || OffsetMs <= -200)
                return NoteGrade.Bad;
            else if (OffsetMs >= 70 || OffsetMs <= -70)
                return NoteGrade.Good;
            else
                return NoteGrade.Perfect;
        }
    }
}
