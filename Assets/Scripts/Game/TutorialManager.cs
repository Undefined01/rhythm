using System;
using System.Collections.Generic;
using UnityEngine;

using SonicBloom.Koreo;
using SonicBloom.Koreo.Players;

public class TutorialManager : TrackManager
{
    int current = 0;
    int groupCnt = 1;
    List<Action> gen;

    void Start()
    {
        // base.Start();
        // gen = new List<Action> {
        //     () => {
        //         var currentSample = koreography.GetLatestSampleTime();
        //         var info = new NoteInfo {
        //             Track = 1,
        //             NoteType = NoteType.Single,
        //             NoteStyle = NoteStyle.Normal,
        //             Group = 0,
        //             AppearedAtSample = currentSample + koreography.SampleRate / 2,
        //             ShouldHitAtSample = currentSample + koreography.SampleRate / 2 + 2 * koreography.SampleRate,
        //             AppearedAtPos = new Vector3(0, 0, 10),
        //             ShouldHitAtPos = new Vector3(0, 0, 0),
        //         };
        //         Debug.Log(info);
        //         Debug.Log(info.AppearedAtSample);
        //         AddNoteInfo(info);
        //     },
        //     () => {
        //         var currentSample = koreography.GetLatestSampleTime();
        //         var info = new NoteInfo {
        //             Track = 1,
        //             NoteType = NoteType.Single,
        //             NoteStyle = NoteStyle.Normal,
        //             Group = groupCnt++,
        //             AppearedAtSample = currentSample + koreography.SampleRate / 2,
        //             ShouldHitAtSample = currentSample + koreography.SampleRate / 2 + 2 * koreography.SampleRate,
        //             AppearedAtPos = new Vector3(0, 0, 10),
        //             ShouldHitAtPos = new Vector3(0, 0, 0),
        //         };
        //         AddNoteInfo(info);
        //         info = new NoteInfo(info);
        //         info.NoteType = NoteType.Hold;
        //         info.AppearedAtSample += koreography.SampleRate / 2;
        //         info.ShouldHitAtSample += koreography.SampleRate / 2;
        //         AddNoteInfo(info);
        //         info = new NoteInfo(info);
        //         info.NoteType = NoteType.Hold;
        //         info.AppearedAtSample += koreography.SampleRate / 2;
        //         info.ShouldHitAtSample += koreography.SampleRate / 2;
        //         AddNoteInfo(info);
        //     },
        //     () => {
        //         var currentSample = koreography.GetLatestSampleTime();
        //         var info = new NoteInfo {
        //             Track = 1,
        //             NoteType = NoteType.Hold,
        //             NoteStyle = NoteStyle.Normal,
        //             Group = groupCnt++,
        //             AppearedAtSample = currentSample + koreography.SampleRate / 2,
        //             ShouldHitAtSample = currentSample + koreography.SampleRate / 2 + 2 * koreography.SampleRate,
        //             AppearedAtPos = new Vector3(0, 0, 10),
        //             ShouldHitAtPos = new Vector3(0, 0, 0),
        //         };
        //         AddNoteInfo(info);
        //         info = new NoteInfo(info);
        //         info.NoteType = NoteType.Hold;
        //         info.AppearedAtSample += koreography.SampleRate / 2;
        //         info.ShouldHitAtSample += koreography.SampleRate / 2;
        //         info.ShouldHitAtPos = new Vector3(-3.3f, 0, 0);
        //         AddNoteInfo(info);
        //         info = new NoteInfo(info);
        //         info.NoteType = NoteType.Hold;
        //         info.AppearedAtSample += koreography.SampleRate / 2;
        //         info.ShouldHitAtSample += koreography.SampleRate / 2;
        //         info.ShouldHitAtPos = new Vector3(3.3f, 0, 0);
        //         AddNoteInfo(info);
        //         info = new NoteInfo(info);
        //         info.NoteType = NoteType.Hold;
        //         info.AppearedAtSample += koreography.SampleRate / 2;
        //         info.ShouldHitAtSample += koreography.SampleRate / 2;
        //         info.ShouldHitAtPos = new Vector3(-3.3f, 0, 0);
        //         AddNoteInfo(info);
        //         info = new NoteInfo(info);
        //         info.NoteType = NoteType.Hold;
        //         info.AppearedAtSample += koreography.SampleRate / 2;
        //         info.ShouldHitAtSample += koreography.SampleRate / 2;
        //         info.ShouldHitAtPos = new Vector3(3.3f, 0, 0);
        //         AddNoteInfo(info);
        //     },
        // };
        // gen[0]();
    }

    protected override void HandleVerdict(object sender, NoteVerdict verdict)
    {
        base.HandleVerdict(sender, verdict);
        if (verdict.Grade != NoteGrade.Perfect)
        {
            // simplePlayer.LoadSong(koreography, 0);
        }
        else
        {
            if (current + 1 < gen.Count)
                current++;
        }
        gen[current]();
    }
}
