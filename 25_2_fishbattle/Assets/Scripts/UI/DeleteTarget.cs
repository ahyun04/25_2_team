using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteTarget : MonoBehaviour
{
    public GameObject target;  // 삭제하고 싶은 오브젝트

    public void DeleteObject()
    {
        if (target != null)
        {
            Destroy(target);
        }
    }
}
