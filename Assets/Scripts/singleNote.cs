using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class singleNote : MonoBehaviour
{
    void Start()
    {
    }

    void FixedUpdate()
    {
        transform.Translate(new Vector3(0, 0, -10) * Time.deltaTime, Space.World);
        if (transform.position.z < 0)
        {
            Destroy(this.gameObject);
        }
    }
}
