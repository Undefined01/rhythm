using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml.Serialization;

public class MainUI : UIController
{
    public Text tips;

    int tipsCurrent = 0;
    string[] tipsStr = { "你可以通过通关曲目解锁他们的故事", "点击右下角调节音效和偏移量等",
                         "点击剧情回看已解锁的演义故事" };

    void Start()
    {
        UpdateTips();
    }

    void OnGUI()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UpdateTips();
        }
    }

    void UpdateTips()
    {
        if (!SaveManager.Save.Story.Any(x => x.Watched))
        {
            tips.text = "点击剧情进入序章";
        }
        else
        {
            tips.text = tipsStr[tipsCurrent];
            tipsCurrent = (tipsCurrent + 1) % tipsStr.Length;
        }
    }
}
