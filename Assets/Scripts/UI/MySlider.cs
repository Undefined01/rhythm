using UnityEngine;
using UnityEngine.EventSystems;

public class MySlider : MonoBehaviour, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// 被用户拖动的操纵杆
    /// </summary>
    [SerializeField]
    private Transform Target;

    [SerializeField]
    private float MinValue = 0f;
    [SerializeField]
    private float MaxValue = 1f;
    [SerializeField]
    private bool ResetPositionAfterRelease = false;

    public float Value;

    void Start()
    {
    }

    /// <summary>
    /// 当操纵杆被拖动时触发
    /// </summary>
    public void OnDrag(PointerEventData data)
    {
        //获取摇杆的RectTransform组件，以检测操纵杆是否在摇杆内移动
        RectTransform draggingPlane = this.transform as RectTransform;
        Vector3 mousePos;

        //检查拖动的位置是否在拖动rect内，
        //然后设置全局鼠标位置并将其分配给操纵杆
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera,
                                                                    out mousePos))
        {
            Target.position = mousePos;
        }

        float posX = Target.localPosition.x;

        if (posX > draggingPlane.sizeDelta.x)
            posX = draggingPlane.sizeDelta.x;
        else if (posX < 0)
            posX = 0;
        Target.localPosition = new Vector3(posX, 0, 0);
        Value = Mathf.Lerp(MinValue, MaxValue, posX / draggingPlane.sizeDelta.x);
    }

    /// <summary>
    /// 当操纵杆结束拖动时触发
    /// </summary>
    public void OnEndDrag(PointerEventData data)
    {
        if (ResetPositionAfterRelease)
        {
            Value = 0;
            Target.position = transform.position;
        }
    }
}