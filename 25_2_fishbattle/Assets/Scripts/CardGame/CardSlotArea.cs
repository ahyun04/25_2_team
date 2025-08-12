using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSlotArea : MonoBehaviour
{
    #region 레퍼런스
    public bool IsCardInside { get; private set; }

    [Header("슬롯 설정")]
    public Transform[] slots;        // 슬롯 위치들
    public LayerMask teamLayer;      // 이 존이 허용하는 팀

    // 슬롯 점유 상태 저장
    protected Dictionary<Transform, GameObject> slotOccupants = new Dictionary<Transform, GameObject>();

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
        if (other.CompareTag("Card") && IsSameTeam(other.gameObject))
        {
            IsCardInside = true;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Card") && IsSameTeam(other.gameObject))
        {
            IsCardInside = false;
        }
    }

    #endregion

    #region 슬롯/카드 레이어
    protected bool IsSameTeam(GameObject card)
    {
        return (teamLayer.value & (1 << card.layer)) != 0;
    }

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
