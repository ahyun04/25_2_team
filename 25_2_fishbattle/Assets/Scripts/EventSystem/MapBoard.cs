using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBoard : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
        }
    }
}
