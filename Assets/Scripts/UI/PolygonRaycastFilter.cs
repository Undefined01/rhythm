using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PolygonRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    PolygonCollider2D bounding;

    void Start()
    {
        bounding = this.GetComponent<PolygonCollider2D>() ?? throw new KeyNotFoundException();
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        return bounding.OverlapPoint(screenPoint);
    }
}
