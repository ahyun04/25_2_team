using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenInventoryUI : MonoBehaviour
{
    public GameObject inventory_Panel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        inventory_Panel.SetActive(!inventory_Panel.activeSelf);
    }
}
