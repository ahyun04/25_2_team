using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    // 카드 타입 열거형 추가
    public enum CardType
    {
        Attack,
        Heal,
        Buff,
        Utility
    }

    public string cardName;                     // 카드 이름
    public int manaCost;                        // 마나 비용
    public int effectAmount;                    // 공격력/효과 값
    public CardType cardType;                   // 카드 타입
    public bool isPlayerCard;

    public Color GetCardColor()                 // 타입에 따른 카드 생성
    {
        switch (cardType)
        {
            case CardType.Attack:
                return new Color(0.9f, 0.3f, 0.3f);                     // 빨강
            case CardType.Heal:
                return new Color(0.3f, 0.9f, 0.3f);                     // 녹색
            case CardType.Buff:
                return new Color(0.3f, 0.3f, 0.9f);                     // 파랑
            case CardType.Utility:
                return new Color(0.9f, 0.9f, 0.3f);                     // 노랑
            default:
                return Color.white;
        }
    }
}