using System.Linq;
using System.Collections;
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

    public List<GameObject> RootOfUi;
    public List<IUIController> ControllerOfUi;

    int currentUi;

    void Awake()
    {
        RootOfUi.ForEach(x => x.SetActive(false));
        RootOfUi[0].SetActive(true);
        ControllerOfUi = RootOfUi.Select(o => o.GetComponent<IUIController>()).ToList();

        currentUi = 0;
    }

    public void SwitchToUi(string ui)
    {
        if (switching)
            return;

        Debug.Log("Switch to");

        int nextUi = RootOfUi.FindIndex(x => x.name == ui);
        Assert.AreNotEqual(nextUi, -1);
        if (ControllerOfUi[currentUi] != null)
        {
            ControllerOfUi[currentUi].OnExit(RootOfUi[nextUi]);
        }
        else
        {
            RootOfUi[currentUi].SetActive(false);
            RootOfUi[nextUi].SetActive(true);
            ControllerOfUi[nextUi]?.OnEnter(RootOfUi[currentUi]);
        }
        currentUi = nextUi;
    }
}

public interface IUIController
{
    void OnEnter(GameObject prevUi);
    void OnExit(GameObject nextUi);
}
