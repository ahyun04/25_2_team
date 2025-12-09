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

    public bool IsPlayerUnit { get; private set; }
    public bool IsDead { get; private set; } = false;
    #endregion

    #region 초기화
    public void Setup(FishSO data, bool isPlayer)
    {
        _cardData = data;
        _currentHp = _cardData.Hp;
        IsPlayerUnit = isPlayer;

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

        if (CardData.Position != Position.Defence)
        {
            FishUnit benchTanker = CardManager.Instance.GetBenchTanker(this.IsPlayerUnit);

            if (benchTanker != null && !benchTanker.IsDead)
            {
                Debug.Log($"<color=green>[방어 발동]</color> {benchTanker.CardData.Name}(벤치)가 {CardData.Name} 대신 {damage} 데미지를 입습니다!");

                benchTanker.TakeDirectDamage(damage);

                // 이펙트 효과 (선택사항)
                // EffectManager.Instance.PlayEffect("ShieldBlock", transform.position); 
                return;
            }
        }

        TakeDirectDamage(damage);
    }

    public void TakeDirectDamage(int damage)
    {
        if (IsDead) return;

        _currentHp -= damage;
        Debug.Log($"{_cardData.Name} ({(IsPlayerUnit ? "Player" : "Enemy")}) 피격! 남은 체력: {_currentHp}");

        UpdateTooltipHp();

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    // 힐 받는 함수
    public void Heal(int amount)
    {
        if (IsDead) return;

        int prevHp = _currentHp;
        _currentHp = Mathf.Min(_currentHp + amount, _cardData.Hp);

        Debug.Log($"<color=yellow>[회복]</color> {_cardData.Name} 체력 회복! ({prevHp} -> {_currentHp})");

        UpdateTooltipHp();
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

        if (IsPlayerUnit)
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
