using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCombat : MonoBehaviour
{
    private CardDisplay _cardDisplay;
    private FishSO fishData;

    public int CurrentHp { get; private set; }

    private void Awake()
    {
        _cardDisplay = GetComponent<CardDisplay>();

        //fishData = _cardDisplay.fishData;

        if (fishData != null)
            CurrentHp = fishData.Hp;
    }

    public void Setup(FishSO data)
    {
        fishData = data;
        CurrentHp = data.Hp;
    }

    public void TakeDamage(int dmg)
    {
        CurrentHp -= dmg;
        Debug.Log($"{fishData.Name} 이(가) {dmg} 피해를 받음! 남은 HP: {CurrentHp}");

        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        CurrentHp += amount;
        if (CurrentHp > fishData.Hp)
            CurrentHp = fishData.Hp;

        Debug.Log($"{fishData.Name} 이(가) {amount} 회복함! 현재 HP: {CurrentHp}");
    }

    private void Die()
    {
        Debug.Log($"{fishData.Name} 이(가) 쓰러졌습니다!");
        Destroy(gameObject);
    }
}
