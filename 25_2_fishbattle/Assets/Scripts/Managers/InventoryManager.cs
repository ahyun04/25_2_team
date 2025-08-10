using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGDatabase;

public class InventoryManager : SingletonMono<InventoryManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => false;

    private FishingMiniGame _game;

    [SerializeField] private GameObject _inventoryUI;

    #endregion

    #region 초기화
    private void Start()
    {
        _game = FindObjectOfType<FishingMiniGame>();
    }

    private void Update()
    {
        if (!_game.IsFishing && Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    #endregion

    #region 인벤토리UI 관리
    public void ToggleInventory()
    {
        _inventoryUI.SetActive(!_inventoryUI.activeSelf);
    }

    public int GetFishCount(FishSO fish)
    {
        var inventoryHolder = FindObjectOfType<InventoryHolder>(); // 또는 직접 할당
        if (inventoryHolder == null || fish == null)
        {
            Debug.LogWarning("InventoryHolder 또는 targetItem이 null입니다.");
            return 0;
        }

        int total = 0;

        foreach (var slot in inventoryHolder.InventorySystem.InventorySlots)
        {
            if (slot.ItemData == fish)
            {
                total += slot.StackSize;
            }
        }

        return total;
    }

    #endregion
}
