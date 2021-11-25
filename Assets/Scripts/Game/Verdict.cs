using System;
using System.Collections.Generic;

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

    public static NoteVerdict FromSample(int sample) => new NoteVerdict(Config.SampleToMs(sample));
    public static NoteVerdict Miss => new NoteVerdict((int)Config.MaxAdvanceHit.TotalMilliseconds);
}

public class VerdictStatistics
{
    int totalOffset = 0;
    public int Count { get; protected set; } = 0;
    int[] verdictCount = new int[4];

    public bool FullCombo => verdictCount[(int)NoteGrade.Miss] == 0;
    public bool AllPerfect => verdictCount[(int)NoteGrade.Perfect] == Count;

    public int MaxCombo { get; protected set; } = 0;
    public int CurrentCombo { get; protected set; } = 0;

    public double AverageOffset => Count == 0 ? 1 : totalOffset / Count;
    public double Accuracy => (verdictCount[0] * 100 + verdictCount[1] * 65 + verdictCount[2] * 25) / Count;

    public void Add(NoteVerdict verdict)
    {
        totalOffset += verdict.OffsetMs;
        Count++;
        verdictCount[(int)verdict.Grade]++;
        if ((int)verdict.Grade <= (int)NoteGrade.Good)
        {
            CurrentCombo++;
            MaxCombo = MaxCombo > CurrentCombo ? MaxCombo : CurrentCombo;
        }
        else
        {
            CurrentCombo = 0;
        }
    }
}
