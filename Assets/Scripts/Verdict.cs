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
            if (OffsetMs >= 200 || OffsetMs <= -200)
                return NoteGrade.Miss;
            else if (OffsetMs >= 130 || OffsetMs <= -130)
                return NoteGrade.Bad;
            else if (OffsetMs >= 60 || OffsetMs <= -60)
                return NoteGrade.Good;
            else
                return NoteGrade.Perfect;
        }
    }
}
