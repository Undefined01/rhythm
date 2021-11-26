using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayUI : MonoBehaviour
{
    public Text ComboText;
    public Text ScoreText;

    public TrackManager TrackManager;

    void LateUpdate() {
        ComboText.text = $"连击 {TrackManager.Statistics.CurrentCombo}";
        ScoreText.text = $"总分 {TrackManager.Statistics.Score:D6}";
    }
}
