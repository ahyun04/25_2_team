using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishUnit : MonoBehaviour
{
    #region 필드
    [Header("참조")]
    [SerializeField] private FishSO _cardData;
    public FishSO CardData => _cardData;

    [Header("상태")]
    private int _currentHp;
    public int CurrentHp => _currentHp;

    private CardSlotArea _slotArea;

    private bool _isPlayerUnit;
    public bool IsDead { get; private set; } = false;
    #endregion

    #region 초기화
    public void Setup(FishSO data, bool isPlayer)
    {
        _cardData = data;
        _currentHp = _cardData.Hp;
        _isPlayerUnit = isPlayer;

        _slotArea = GetComponentInParent<CardSlotArea>();
        if (_slotArea == null)
        {
            Debug.LogError($"{_cardData.Name} 유닛이 소속된 슬롯 영역(CardSlotArea)을 찾을 수 없습니다!");
        }
    }

    #endregion

    #region 전투 로직
    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        _currentHp -= damage;
        Debug.Log($"{_cardData.Name}이(가) {damage}의 데미지를 받아 현재 HP: {_currentHp}");

        UpdateTooltipHp();

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    private void UpdateTooltipHp()
    {
        if (TryGetComponent<CardDisplay>(out var cardDisplay))
        {
            cardDisplay.UpdateTooltip(CurrentHp);
        }
    }

    public void Die()
    {
        if (IsDead) return; // Die()가 중복 호출되는 것을 방지
        IsDead = true;

        if (_isPlayerUnit)
        {
            TurnManager.Instance.AddKill(false);
        }
        else
        {
            TurnManager.Instance.AddKill(true);
        }

        Transform mySlot = transform.parent;
        if (mySlot != null)
        {
            CardSlotArea area = GetComponentInParent<CardSlotArea>();
            if (area != null)
            {
                area.ReleaseSlot(mySlot);
            }
        }

        gameObject.SetActive(false);

        Destroy(gameObject, 0.1f);
    }

    #endregion
}
