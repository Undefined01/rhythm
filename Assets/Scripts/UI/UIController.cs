using UnityEngine;

public class UIController : MonoBehaviour
{
    public virtual void OnEnter(UIController prevUi, string param)
    {
    }
    public virtual void OnExit(UIController nextUi, string nextParam)
    {
        this.gameObject.SetActive(false);
        nextUi.gameObject.SetActive(true);
        nextUi.OnEnter(this, nextParam);
    }
}
