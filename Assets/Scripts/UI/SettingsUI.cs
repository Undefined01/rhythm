using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : UIController
{
    public MySlider BackgroundLightnessSlider;
    public MySlider NoteHitSoundEffectVolumnSlider;
    public MySlider MusicVolumnSlider;
    public MySlider OffsetSlider;
    public Text OffsetText;

    float Offset;

    void Start()
    {
        BackgroundLightnessSlider.Value = SaveManager.Save.Settings.BackgroundLightness;
        BackgroundLightnessSlider.OnValueChanged += v => SaveManager.Save.Settings.BackgroundLightness = v;
        Debug.Log(SaveManager.Save.Settings.BackgroundLightness);
        NoteHitSoundEffectVolumnSlider.Value = SaveManager.Save.Settings.HitSoundEffectVolumn;
        NoteHitSoundEffectVolumnSlider.OnValueChanged += v => SaveManager.Save.Settings.HitSoundEffectVolumn = v;
        MusicVolumnSlider.Value = SaveManager.Save.Settings.MusicVolumn;
        MusicVolumnSlider.OnValueChanged += v => SaveManager.Save.Settings.MusicVolumn = v;

        Offset = SaveManager.Save.Settings.HitOffsetMs;

        OffsetSlider.Value = 0;
        OffsetSlider.OnValueChanged += _ => SaveManager.Save.Settings.HitOffsetMs = (int)Offset;
    }

    public override void OnExit(UIController next, string nextParam)
    {
        SaveManager.SaveAll();
        base.OnExit(next, nextParam);
    }

    void LateUpdate()
    {
        if (-300 <= Offset && Offset <= 300)
            Offset += OffsetSlider.Value * Mathf.Abs(OffsetSlider.Value) * Time.deltaTime;
        OffsetText.text = $"{(int)Offset} ms";

        if ((Offset > 100 || Offset < -100) && NoteHitSoundEffectVolumnSlider.Value >= .03f)
            OffsetText.text += "\n偏移量过高，推荐关闭打击音效";
    }
}
