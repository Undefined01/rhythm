using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : UIController
{
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

        if (Input.touches.Count() == 0 && !Input.GetMouseButton(0))
            OffsetSlider.value = 0;
    }

    public void Save()
    {
        SaveManager.Save.Settings.HitOffsetMs = (int)Offset;
        SaveManager.SaveAll();
    }
}
