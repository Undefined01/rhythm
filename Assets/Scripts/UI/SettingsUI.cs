using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : UIController
{
    public Slider NoteHitSoundEffectVolumnSlider;
    public Slider OffsetSlider;
    public Text OffsetText;

    double Offset;

    void Start()
    {
        Offset = SaveManager.Save.Settings.HitOffsetMs;
    }

    void Update()
    {
        if (-300 <= Offset && Offset <= 300)
            Offset += OffsetSlider.value * Mathf.Abs(OffsetSlider.value) * Time.deltaTime;
        OffsetText.text = $"{(int)Offset} ms";

        if ((Offset > 100 || Offset < -100) && NoteHitSoundEffectVolumnSlider.value >= 3)
            OffsetText.text += "\n偏移量过高，推荐关闭打击音效";

        if (Input.touches.Count() == 0 && !Input.GetMouseButton(0))
            OffsetSlider.value = 0;
    }

    public void Save()
    {
        SaveManager.Save.Settings.HitOffsetMs = (int)Offset;
        SaveManager.Save.Settings.HitSoundEffectVolumn = (int)NoteHitSoundEffectVolumnSlider.value;
        SaveManager.SaveAll();
    }
}
