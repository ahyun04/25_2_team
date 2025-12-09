using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuImage : MonoBehaviour
{
    public GameObject menuImage;
    public Button openButton;
    public Button closeButton;

    void Start()
    {
        menuImage.SetActive(false); 

        openButton.onClick.AddListener(OpenMenu);
        closeButton.onClick.AddListener(CloseMenu);
    }

    void OpenMenu()
    {
        menuImage.SetActive(true);
    }

    void CloseMenu()
    {
        menuImage.SetActive(false);
    }
}
