using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    #region 레퍼런스
    [Header("인벤토리 패널")]
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private Button _closePanelButton;

    #endregion

    #region 초기화
    void Start()
    {
        _closePanelButton.onClick.AddListener(() => _inventoryPanel.SetActive(false)); 
    }

    #endregion
}
