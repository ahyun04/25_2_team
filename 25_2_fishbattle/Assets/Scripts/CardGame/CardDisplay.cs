using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    #region 레퍼런스
    [Header("카드 데이터")]
    public FishSO fishData;                         
    public int cardIndex;                             
                  
    [Header("상태")]
    public bool isDragging = false;
    private Vector3 _originalPosition;

    private CardManager _cardManager;                    
    [NonSerialized] public CardSlotArea _currentSlotArea;
    [NonSerialized] public Transform _currentSlot;
    private Transform _originalParent;

    [Header("툴팁")]
    [SerializeField] private GameObject tooltipPrefab;

    #endregion

    #region 초기화
    void Start()
    {
        _cardManager = FindObjectOfType<CardManager>();
        _currentSlotArea = FindObjectOfType<CardSlotArea>();

        if (tooltipPrefab != null)
        {
            GameObject tooltipInstance = Instantiate(tooltipPrefab, transform);

            // 플레이어/적 카드에 따라 툴팁 설정
            if (fishData.IsPlayerCard)
            {
                tooltipInstance.transform.localPosition = new Vector3(0, -0.1f, 0);
                tooltipInstance.transform.localRotation = Quaternion.Euler(0, 90, 180);
            }
            else // 적 카드
            {
                tooltipInstance.transform.localPosition = new Vector3(0, 0.2f, -1.3f);
                tooltipInstance.transform.localRotation = Quaternion.Euler(0, -90, 0);
            }

            tooltipInstance.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            // 생성된 툴팁을 비활성화
            tooltipInstance.SetActive(false);
        }
        if (fishData.IsPlayerCard)
            gameObject.tag = "PlayerCard";
        else
            gameObject.tag = "EnemyCard";

        SetupCard(fishData);
    }

    // 카드 데이터 설정
    public void SetupCard(FishSO data)
    {
        fishData = data;
    }

    #endregion

    #region 마우스 클릭/드래그
    private void OnMouseDown()
    {
        // 플레이어 카드일 경우에만 드래그 시작
        if (!fishData.IsPlayerCard) return;

        if (CardManager.Instance.IsHandExpanded() || CardManager.Instance.IsCardFocused(gameObject)) return;

        // 드래그 시작 시 기존 슬롯 해제
        if (_currentSlotArea != null && _currentSlot != null)
        {
            _currentSlotArea.ReleaseSlot(_currentSlot);
            _currentSlot = null;
        }

        _currentSlotArea = null;

        isDragging = true;
        _originalPosition = transform.position;

        // 드래그 시작 시 부모를 잠시 해제
        _originalParent = transform.parent;
        transform.SetParent(null);
    }

    private void OnMouseDrag()
    {
        if (!fishData.IsPlayerCard || !isDragging) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
    }

    private void OnMouseUp()
    {
        if (!fishData.IsPlayerCard || !isDragging) return;

        isDragging = false;

        if (_currentSlotArea != null && _currentSlotArea is BattlePos &&
            fishData.Description.ToLower().Contains("healer"))
        {
            Debug.Log($"[{fishData.Name}]은 힐러라서 배틀 위치에 올릴 수 없습니다.");

            transform.SetParent(_originalParent);
            transform.DOMove(_originalPosition, 0.2f).SetEase(Ease.OutCubic);
            return;
        }

        // 카드 배치 시도
        if (_currentSlotArea != null && _currentSlotArea.IsCardInside)
        {
            if (!TurnManager.Instance.IsGameStarted)
            {
                if (fishData.IsPlayerCard)
                {
                    _cardManager.SetupInitialPlayerCard(this.cardIndex);
                }
            }
            else
            {
                if (_cardManager.PlayCardFromHand(true, this.cardIndex))
                {
                    Debug.Log("카드를 성공적으로 배치했습니다.");
                }
                else
                {
                    ReturnToOriginalPosition("카드 배치 실패. AP 부족 또는 슬롯 없음.");
                }
            }
        }
        else
        {
            ReturnToOriginalPosition("유효한 슬롯이 아니어서 복귀.");
        }

        _currentSlotArea = null;
    }

    private void ReturnToOriginalPosition(string reason)
    {
        transform.SetParent(_originalParent);
        transform.DOMove(_originalPosition, 0.2f).SetEase(Ease.OutCubic);
        Debug.Log(reason);
    }

    #endregion

    #region 공간 트리거 관련
    private void OnTriggerEnter(Collider other)
    {
        if (!isDragging) return; 

        CardSlotArea area = other.GetComponent<CardSlotArea>();
        if (area != null)
        {
            // 태그 검사
            if (other.CompareTag("PlayerCard") && gameObject.CompareTag("PlayerCard") ||
                other.CompareTag("EnemyCard") && gameObject.CompareTag("EnemyCard"))
            {
                _currentSlotArea = area;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isDragging) return;

        CardSlotArea area = other.GetComponent<CardSlotArea>();
        if (area != null)
        {
            if (other.CompareTag(gameObject.tag))
                _currentSlotArea = area;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isDragging) return;

        CardSlotArea area = other.GetComponent<CardSlotArea>();
        if (area != null && area == _currentSlotArea)
        {
            _currentSlotArea = null;
        }
    }

    #endregion

    #region 툴팁
    public void SetTooltipActive(bool isActive, bool isFocus)
    {
        if (transform.childCount > 0)
        {
            GameObject tooltipObject = transform.GetChild(0).gameObject;
            if (tooltipObject != null)
            {
                tooltipObject.SetActive(isActive);

                if (isActive && tooltipObject.TryGetComponent<Tooltip>(out var tooltip))
                {
                    tooltip.SetupTooltip(fishData.Name, fishData.Hp, fishData.Skill_name, fishData.AbilityToAct);

                    if (tooltip._nameText != null)
                    {
                        tooltip._nameText.gameObject.SetActive(isFocus);
                    }
                }
            }
        }
    }

    public void UpdateTooltip(int currentHp)
    {
        // 툴팁 자식 오브젝트를 찾습니다.
        if (transform.childCount > 0)
        {
            GameObject tooltipObject = transform.GetChild(0).gameObject;

            // 툴팁 컴포넌트를 찾아 HP를 업데이트합니다.
            if (tooltipObject != null && tooltipObject.TryGetComponent<Tooltip>(out var tooltip))
            {
                tooltip.UpdateHpText(currentHp);
            }
        }
    }

    #endregion
}