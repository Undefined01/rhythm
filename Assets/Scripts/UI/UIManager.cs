using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class UIManager : MonoBehaviour
{
    bool enteringUi = false;
    bool exitingUi = false;
    bool switching
    {
        get => enteringUi || exitingUi;
    }

    public List<UIController> Ui;
    public AudioSource UiSoundEffect;

    public Image BackgroundImage;
    public GameObject BackgroundMask;

    UIController currentUi;

    public static UIManager Instance { get; protected set; }

    void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
        Ui.ForEach(x => x.gameObject.SetActive(false));
        currentUi = Ui[0];
        currentUi.gameObject.SetActive(true);
    }

    void Start()
    {
        var brightness = SaveManager.Save.Settings.BackgroundBrightness;
        BackgroundImage.color = new Color(brightness, brightness, brightness);
    }

    public void SwitchToUi(string ui)
    {
        if (switching)
            return;

        var uiSplit = ui.Split(';');
        var uiName = uiSplit[0];
        string uiParam = null;
        if (uiSplit.Length >= 2)
            uiParam = uiSplit[1];
        Debug.Log($"Switch to {uiName} with {uiParam}");

        var nextUi = Ui.Single(x => x.gameObject.name == uiName);
        Assert.IsNotNull(nextUi);

        currentUi.OnExit(nextUi, uiParam);
        currentUi = nextUi;
    }

    public void PlayUiSoundEffect()
    {
        UiSoundEffect.volume = (float)SaveManager.Save.Settings.MusicVolumn;
        UiSoundEffect.Play();
    }

    public void ChangeBackgroundImage(Sprite image)
    {
        BackgroundImage.sprite = image;
    }
    public void SetBackgroundMask(bool isActive)
    {
        BackgroundMask.SetActive(isActive);
    }
}
