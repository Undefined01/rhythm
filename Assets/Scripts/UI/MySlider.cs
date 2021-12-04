using System;
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

    [SerializeField]
    private float _value;
    public float Value
    {
        get => _value;
        set {
            _value = value;
            Debug.Log($"Slider set to {_value}");
            Target.localPosition = new Vector3((_value - MinValue) / (MaxValue - MinValue) * width, 0, 0);
        }
    }

    public event Action<float> OnChanging;
    public event Action<float> OnValueChanged;

    private float width;

    void Awake()
    {
        width = (transform as RectTransform).rect.width;
    }

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

        posX = Mathf.Clamp(posX, 0, width);
        Target.localPosition = new Vector3(posX, 0, 0);
        _value = Mathf.Lerp(MinValue, MaxValue, posX / width);
        OnChanging?.Invoke(_value);
    }

    public void OnEndDrag(PointerEventData data)
    {
        OnValueChanged?.Invoke(Value);
        if (ResetPositionAfterRelease)
            Value = 0;
    }
}