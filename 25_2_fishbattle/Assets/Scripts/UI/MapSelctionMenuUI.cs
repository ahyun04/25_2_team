using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSelctionMenuUI : MonoBehaviour
{
    public GameObject ESC_Panel;

    void Start()
    {
        ESC_Panel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePanel();
    }

    private void TogglePanel()
    {
        ESC_Panel.SetActive(!ESC_Panel.activeSelf);
    }
}
