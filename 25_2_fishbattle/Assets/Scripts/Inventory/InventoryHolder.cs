using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InventoryHolder : MonoBehaviour
{
    #region 레퍼런스
    [SerializeField] private int _inventorySize;
    [SerializeField] protected InventorySystem inventorySystem;

    public InventorySystem InventorySystem => inventorySystem;

    public static UnityAction<InventorySystem> OnDynamicInventoryDisplayRequested;

    #endregion

    #region 초기화
    private void Awake()
    {
        inventorySystem = new InventorySystem(_inventorySize);
    }

    #endregion
}
