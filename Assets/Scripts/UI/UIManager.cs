using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
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

    UIController currentUi;

    void Awake()
    {
        Ui.ForEach(x => x.gameObject.SetActive(false));
        currentUi = Ui[0];
        currentUi.gameObject.SetActive(true);
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
    }
}
