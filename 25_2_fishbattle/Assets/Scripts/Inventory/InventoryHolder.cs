using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InventoryHolder : SingletonMono<InventoryHolder>
{
    #region 레퍼런스
    protected override bool DontDestroy => true;

    [SerializeField] private int _inventorySize;
    [SerializeField] protected InventorySystem inventorySystem;

    public InventorySystem InventorySystem => inventorySystem;

    public static UnityAction<InventorySystem> OnDynamicInventoryDisplayRequested;

    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();

        inventorySystem = new InventorySystem(_inventorySize);
    }

    #endregion
}
