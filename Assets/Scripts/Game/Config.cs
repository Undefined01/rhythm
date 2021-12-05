using System;

using UnityEngine;
using SonicBloom.Koreo;

public static class Config
{
    public static TimeSpan MaxAdvanceHit = TimeSpan.FromMilliseconds(200);
    public static TimeSpan MaxDelayHit = TimeSpan.FromMilliseconds(200);
    public static TimeSpan Offset = TimeSpan.FromMilliseconds(0);

    public static Vector3 NoteStartPos = new Vector3(0, 2, 10);

    public static Koreography Koreography;
    public static int SampleRate;
    public static int MaxAdvanceHitSample;
    public static int MaxDelayHitSample;
    public static int OffsetSample;

    public static int CurrentSample => Koreography?.GetLatestSampleTime() ?? 0;

    public static void Set(Koreography koreography)
    {
        Koreography = koreography;

        Offset = TimeSpan.FromMilliseconds(SaveManager.Save.Settings.HitOffsetMs);

        SampleRate = koreography.SampleRate;
        MaxAdvanceHitSample = TimeToSample(MaxAdvanceHit);
        MaxDelayHitSample = TimeToSample(MaxDelayHit);
        OffsetSample = TimeToSample(Offset);
    }

    public static int TimeToSample(TimeSpan time) => (int)(time.TotalSeconds * SampleRate);
    public static int MsToSample(int ms) => (int)(ms * SampleRate / 1000);
    public static int SampleToMs(int sample) => sample * 1000 / SampleRate;
}
