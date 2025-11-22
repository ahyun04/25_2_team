using UnityEngine;
using System.Collections.Generic;

public class TradeManager : SingletonMono<TradeManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => true;

    [Header("레퍼런스")]
    public InventoryUIController inventoryPanel;
    public FishDatabaseSO fishDatabase;         // 모든 물고기 정보

    // 현재 교환 상태
    private bool _isTrading = false;
    private FishSO _npcOfferItem;               // NPC가 주기로 한 아이템

    #endregion

    #region 물물교환
    public void StartTrade()
    {
        _isTrading = true;

        // NPC가 줄 아이템 결정 (랜덤)
        _npcOfferItem = GetRandomFishFromDatabase();

        if (_npcOfferItem == null)
        {
            Debug.LogError("교환할 아이템을 찾지 못했습니다.");
            EndTrade();
            return;
        }

        Debug.Log($"NPC 제안: {_npcOfferItem.Name}을 줄게. 너는 뭘 줄래?");

        // 플레이어 인벤토리 열기
        inventoryPanel.gameObject.SetActive(true);

        // 나중에 추가할거 : UI에 "NPC가 [아이템이름]을 주려고 합니다. 교환할 아이템을 선택하세요." 같은 텍스트 띄우기
    }

    public void OnPlayerItemSelected(InventorySlot_UI slotUI)
    {
        if (!_isTrading) return;
        if (slotUI.AssignedItem == null) return; // 빈 슬롯 클릭 무시

        // 3. 교환 실행
        ProcessTrade(slotUI.AssignedItem);
    }

    private void ProcessTrade(FishSO playerOfferItem)
    {
        // 플레이어 아이템 제거 (-1)
        InventoryHolder.Instance.InventorySystem.RemoveItem(playerOfferItem, 1);

        // NPC 아이템 지급 (+1)
        bool success = InventoryHolder.Instance.InventorySystem.AddToInventory(_npcOfferItem, 1);

        if (success)
        {
            Debug.Log($"교환 성공! {playerOfferItem.Name} <-> {_npcOfferItem.Name}");
            // 성공 메세지 띄우기 or 대화창으로 결과 알려주기
        }
        else
        {
            Debug.LogWarning("인벤토리가 꽉 차서 NPC 아이템을 받을 수 없습니다!");
            // 실패 처리 (플레이어 아이템을 다시 돌려주거나 등등)
            InventoryHolder.Instance.InventorySystem.AddToInventory(playerOfferItem, 1); // 롤백
        }

        EndTrade();
    }

    public void EndTrade()
    {
        _isTrading = false;
        _npcOfferItem = null;
        inventoryPanel.gameObject.SetActive(false);

        DialogManager.Instance.EndDialog();
    }

    // 확률에 따라 NPC 아이템 뽑기 (단순 랜덤)
    private FishSO GetRandomFishFromDatabase()
    {
        if (fishDatabase == null || fishDatabase.fishItems.Count == 0) return null;
        int randomIndex = Random.Range(0, fishDatabase.fishItems.Count);
        return fishDatabase.fishItems[randomIndex];
    }

    #endregion
}