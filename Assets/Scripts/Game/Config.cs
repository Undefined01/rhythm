using System;

using UnityEngine;
using SonicBloom.Koreo;

public static class Config
{
    public static TimeSpan MaxAdvanceHit = TimeSpan.FromMilliseconds(200);
    public static TimeSpan MaxDelayHit = TimeSpan.FromMilliseconds(200);

    public static Koreography Koreography;
    public static int SampleRate;
    public static int MaxAdvanceHitSample;
    public static int MaxDelayHitSample;

    public static void Set(Koreography koreography) {
        Koreography = koreography;

        SampleRate = koreography.SampleRate;
        MaxAdvanceHitSample = (int)(MaxAdvanceHit.TotalSeconds * SampleRate);
        MaxDelayHitSample = (int)(MaxDelayHit.TotalSeconds * SampleRate);
    }
}
