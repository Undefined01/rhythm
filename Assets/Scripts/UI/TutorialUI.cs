using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : UIController
{
    public TrackManager TrackManager;
    public UIManager UIManager;
    public GameObject Tip;
    public Text TipText;

    int current = 0;

    List<Stage> stages;

    List<Action> stateMachine = new List<Action>();
    int DisableClickForFrames;

    class Stage
    {
        public List<string> tips;
        public List<NoteInfo> notes;
    }

    void InitStages()
    {
        int MsToSample(int ms) => ms * 44100 / 1000;

        NoteInfo GenNote(int delayMs)
        {
            return new NoteInfo {
                Track = 1,
                NoteType = NoteType.Single,
                NoteStyle = NoteStyle.Normal,
                Group = 0,
                AppearedAtSample = MsToSample(delayMs + 500),
                ShouldHitAtSample = MsToSample(delayMs + 2500),
                AppearedAtPos = new Vector3(0, 0, 10),
                ShouldHitAtPos = new Vector3(0, 0, 0),
            };
        }

        stages = new List<Stage>();
        {
            var stage = new Stage {
                tips =
                    new List<string> {
                        "先来最简单的音符。只需要在到达末端时单次点击即可。",
                    },
                notes = Enumerable.Range(0, 4).Select(i => GenNote(i * 2000)).ToList(),
            };
            stages.Add(stage);
        }
        {
            var list = new List<NoteInfo>();
            foreach (var i in Enumerable.Range(0, 3))
            {
                list.Add(GenNote(i * 5000));
                list.AddRange(Enumerable.Range(1, 3).Select(j => {
                    var o = GenNote(i * 5000);
                    o.NoteType = NoteType.Hold;
                    o.AppearedAtSample += MsToSample(j * 500);
                    o.ShouldHitAtSample += MsToSample(j * 500);
                    return o;
                }));
            }
            var stage = new Stage {
                tips =
                    new List<string> {
                        "这是长按音符。中途不要松手。",
                    },
                notes = list,
            };
            stages.Add(stage);
        }
        {
            var list = new List<NoteInfo>();
            foreach (var i in Enumerable.Range(0, 3))
            {
                list.Add(GenNote(i * 5000));
                list.AddRange(Enumerable.Range(1, 3).Select(j => {
                    var o = GenNote(i * 5000);
                    o.ShouldHitAtPos = new Vector3(j % 2 == 0 ? -2.2f : 2.2f, 0, 0);
                    o.NoteType = NoteType.Hold;
                    o.AppearedAtSample += MsToSample(j * 500);
                    o.ShouldHitAtSample += MsToSample(j * 500);
                    return o;
                }));
            }
            var stage = new Stage {
                tips =
                    new List<string> {
                        "音符和轨道偶尔会出现摆动，注意跟上节奏。",
                    },
                notes = list,
            };
            stages.Add(stage);
        }
    }

    void Update()
    {
        if (DisableClickForFrames > 0)
        {
            DisableClickForFrames--;
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.touches.Any(t => t.phase == TouchPhase.Began))
        {
            var action = stateMachine.FirstOrDefault();
            if (action != null)
            {
                stateMachine.RemoveAt(0);
                action.Invoke();
            }
        }
    }

    public override void OnEnter(UIController prevUi, string param)
    {
        if (stages == null)
            InitStages();
        StartStage(0, false);
        TrackManager.OnHandleVerdict += HandleVerdict;
    }

    public override void OnExit(UIController nextUi, string nextParam)
    {
        Debug.Log("Exit");
        TrackManager.CleanUp();
        TrackManager.OnHandleVerdict -= HandleVerdict;

        base.OnExit(nextUi, nextParam);
    }

    void StartStage(int stage, bool tryAgain)
    {
        TrackManager.CleanUp();
        Debug.Log($"Start stage {stage} {tryAgain}");

        current = stage;
        if (stage >= stages.Count)
        {
            UIManager.SwitchToUi("Select Music");
            return;
        }

        Action SetTip(string t) => () =>
        {
            Tip.SetActive(true);
            TipText.text = t;
        };
        stateMachine.Clear();
        if (tryAgain)
            stateMachine.Add(SetTip("别着急，再试一次"));
        stateMachine.AddRange(stages[stage].tips.Select(t => SetTip(t)));
        stateMachine.Add(() => {
            Tip.SetActive(false);
            Debug.Log($"{string.Join("\n", stages[stage].notes.Select(x => x.ShouldHitAtSample))}");
            TrackManager.StartLevelWithNote("empty", stages[stage].notes);
        });
        stateMachine.First().Invoke();
        stateMachine.RemoveAt(0);
        DisableClickForFrames = 5;
    }

    protected void HandleVerdict(object sender, NoteVerdict verdict)
    {
        if (TrackManager.Statistics.Count >= TrackManager.ExpectedHitsInTotal)
        {
            if (TrackManager.Statistics.FullCombo)
            {
                StartStage(current + 1, false);
            }
            else
            {
                StartStage(current, true);
            }
        }
    }
}
