using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardSlotArea : MonoBehaviour
{
    #region 레퍼런스
    public bool IsCardInside { get; private set; }

    [Header("슬롯 설정")]
    public Transform[] slots;        // 슬롯 위치들

    [Header("허용 태그")]
    [SerializeField] private string allowedTag = "PlayerCard";

    // 슬롯 점유 상태 저장
    protected Dictionary<Transform, GameObject> slotOccupants = new Dictionary<Transform, GameObject>();

    public KeyValuePair<Transform, GameObject> GetFirstOccupiedSlotAndUnit()
    {
        foreach (var pair in slotOccupants)
        {
            if (pair.Value != null)
            {
                return pair;
            }
        }
        return default(KeyValuePair<Transform, GameObject>);
    }

    #endregion

    #region 초기화
    protected virtual void Awake()
    {
        foreach (var slot in slots)
            slotOccupants[slot] = null;
    }

    #endregion

    #region 카드 트리거 이벤트
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(allowedTag))
        {
            IsCardInside = true;
            CardManager.Instance.SetTooltipForCard(other.gameObject, true, false);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(allowedTag))
        {
            IsCardInside = false;
            CardManager.Instance.SetTooltipForCard(other.gameObject, false, false);
        }
    }
    #endregion

    #region 슬롯 관리
    public Transform GetNearestEmptySlot(Vector3 cardPos)
    {
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var slot in slots)
        {
            if (slotOccupants[slot] != null) continue; // 점유 중이면 스킵

            float dist = Vector3.Distance(cardPos, slot.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = slot;
            }
        }
        return nearest;
    }

    public void OccupySlot(Transform slot, GameObject card)
    {
        if (slot != null)
            slotOccupants[slot] = card;
    }

    public void ReleaseSlot(Transform slot)
    {
        if (slot != null && slotOccupants.ContainsKey(slot))
            slotOccupants[slot] = null;
    }

    public bool IsSlotOccupied(Transform slot)
    {
        return slotOccupants.ContainsKey(slot) && slotOccupants[slot] != null;
    }

    public List<GameObject> GetOccupiedCards()
    {
        List<GameObject> occupiedCards = new List<GameObject>();
        foreach (var occupant in slotOccupants.Values)
        {
            if (occupant != null)
            {
                occupiedCards.Add(occupant);
            }
        }
        return occupiedCards;
    }
    #endregion
}