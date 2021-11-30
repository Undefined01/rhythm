using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayUI : UIController
{
    public Text ComboText;
    public Text ScoreText;

    public GameObject Result;
    public Text ResultText;

    public TrackManager TrackManager;

    private string levelName;

    void LateUpdate()
    {
        ComboText.text = $"连击 {TrackManager.Statistics.CurrentCombo}";
        ScoreText.text = $"总分 {TrackManager.Statistics.Score:D6}";
    }

    public override void OnEnter(UIController prevUi, string param)
    {
        levelName = param;
        TrackManager.StartLevel(levelName);
        TrackManager.OnFinished += ShowResult;
        Result.SetActive(false);
    }

    public override void OnExit(UIController nextUi, string nextParam)
    {
        TrackManager.OnFinished -= ShowResult;
        base.OnExit(nextUi, nextParam);
    }

    public void ShowResult()
    {
        var statistics = TrackManager.Statistics;
        var grade = statistics.Score switch {
            var x when x >= 900_000 => "φ",
            var x when x >= 800_000 => "A",
            var x when x >= 700_000 => "B",
            _ => "C",
        };

        Result.SetActive(true);
        ResultText.text = $@"总分
{statistics.Score:D6}
评级
<size=20>{grade}</size>
最大连击数
{statistics.MaxCombo}";

        var level = SaveManager.Save.Level.Single(level => level.Name == levelName);
        if (statistics.Score > level.BestScore) {
            level.BestScore = statistics.Score;
        }
        level.FullCombo |= statistics.FullCombo;
        level.AllPerfect |= statistics.AllPerfect;
    }
}
