using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayUI : UIController
{
    public Text ComboText;
    public Text ScoreText;

    public GameObject Result;
    public Text ResMusicNameText;
    public Text ResMusicAuthorText;
    public Text ResComboText;
    public Text ResScoreText;
    public Text ResRatingText;
    public GameObject BestScore;

    public TrackManager TrackManager;

    private string levelName;

    public override void OnEnter(UIController prevUi, string param)
    {
        base.OnEnter(prevUi, param);
        levelName = param;
        TrackManager.StartLevel(levelName);
        TrackManager.OnFinished += ShowResult;
        Result.SetActive(false);
        BestScore.SetActive(false);
        UIManager.Instance.SetBackgroundMask(true);
    }

    public override void OnExit(UIController nextUi, string nextParam)
    {
        TrackManager.OnFinished -= ShowResult;
        TrackManager.CleanUp();
        UIManager.Instance.SetBackgroundMask(false);
        base.OnExit(nextUi, nextParam);
    }

    void LateUpdate()
    {
        ComboText.text = $"连击 {TrackManager.Statistics.CurrentCombo}";
        ScoreText.text = $"总分 {TrackManager.Statistics.Score:D6}";
    }

    public void ShowResult()
    {
        var statistics = TrackManager.Statistics;
        var level = SaveManager.Save.Level.Single(level => level.Name == levelName);

        var rating = statistics.Score switch {
            var x when x >= 1000_000 => "曲高和寡",
            var x when x >= 900_000 => "绕梁之音",
            var x when x >= 800_000 => "正声雅乐",
            var x when x >= 600_000 => "别具一格",
            var x when x >= 500_000 => "轻歌曼舞",
            var x when x >= 300_000 => "差强人意",
            _ => "勇往直前",
        };
        rating = $"{rating[0]}    \n{rating[1]}{rating[2]}\n    {rating[3]}";

        Result.SetActive(true);
        ResComboText.text = statistics.MaxCombo.ToString();
        ResScoreText.text = $"{statistics.Score:D6}";
        ResRatingText.text = rating;
        ResMusicNameText.text = level.Name;
        ResMusicAuthorText.text = level.Author;

        if (statistics.Score > level.BestScore)
        {
            level.BestScore = statistics.Score;
            BestScore.SetActive(true);
        }
        level.FullCombo |= statistics.FullCombo;
        level.AllPerfect |= statistics.AllPerfect;

        SaveManager.SaveAll();
    }
}
