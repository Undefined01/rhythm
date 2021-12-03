using UnityEngine;

public class UIController : MonoBehaviour
{
    public Sprite BackgroundImage;

    public virtual void OnEnter(UIController prevUi, string param)
    {
        if (BackgroundImage != null)
            UIManager.Instance.ChangeBackgroundImage(BackgroundImage);
    }
    public virtual void OnExit(UIController nextUi, string nextParam)
    {
        this.gameObject.SetActive(false);
        nextUi.gameObject.SetActive(true);
        nextUi.OnEnter(this, nextParam);
    }
}
