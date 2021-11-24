using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    bool enteringUi = false;
    bool exitingUi = false;
    bool switching {get => enteringUi || exitingUi; }

    public List<GameObject> RootOfUi;

    GameObject currentUi;

    void Awake() {
        RootOfUi.ForEach(x => x.SetActive(false));
        RootOfUi[0].SetActive(true);
        currentUi = RootOfUi[0];
    }

    public void SwitchToUi(string ui) {
        if (switching) return;

        GameObject nextUi = RootOfUi.Single(x => x.name == ui);
        currentUi.SetActive(false);
        nextUi.SetActive(true);
        currentUi = nextUi;
    }
}
