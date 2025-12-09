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
    private PlayerHandController _handController;
    private CardActionHandler _actionHandler;
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
        _actionHandler = FindObjectOfType<CardActionHandler>();
        _handController = FindObjectOfType<PlayerHandController>();

        if (tooltipPrefab != null)
        {
            GameObject tooltipInstance = Instantiate(tooltipPrefab, transform);

            // 플레이어/적 카드에 따라 툴팁 설정
            if (fishData.IsPlayerCard)
            {
                tooltipInstance.transform.localPosition = new Vector3(0, -0.37f, 3.04f);
                tooltipInstance.transform.localRotation = Quaternion.Euler(180, 90, 90);
            }
            else // 적 카드
            {
                tooltipInstance.transform.localPosition = new Vector3(0, -0.06f, -2.55f);
                tooltipInstance.transform.localRotation = Quaternion.Euler(0, 90, -90);
            }

            tooltipInstance.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);

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
        if (!fishData.IsPlayerCard) return;

        if (_handController.IsHandExpanded() || _handController.IsCardFocused(gameObject)) return;

        bool isFromHand = transform.parent == CardManager.Instance.playerHandPosition;

        bool isFromBench = false;
        foreach (var benchArea in CardManager.Instance.playerBenchAreas)
        {
            if (transform.parent == benchArea.transform)
            {
                isFromBench = true;
                break;
            }
        }

        if (isFromHand)
        {
            // 손패에 있는 카드라면, 항상 드래그 가능
        }
        else if (isFromBench)
        {
            // 벤치에 있는 카드라면, 레이어가 9번일 때만 드래그 허용
            if (gameObject.layer != 9)
            {
                Debug.Log("배틀필드에 빈 슬롯이 없어 벤치 카드를 움직일 수 없습니다.");
                return;
            }
        }
        else
        {
            return;
        }

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

        // 유효한 슬롯에 드롭했는지 확인
        if (_currentSlotArea != null && _currentSlotArea.IsCardInside)
        {
            bool isBenchOnlyUnit = fishData.Position == Position.Defence ||
                               fishData.Position == Position.Heal ||
                               fishData.Position == Position.Support;

            if (_currentSlotArea is BattlePos && isBenchOnlyUnit)
            {
                ReturnToOriginalPosition($"[{fishData.Position}] 포지션은 배틀 필드에 배치할 수 없습니다. (벤치 전용)");
                return;
            }

            if (_originalParent == _cardManager.playerHandPosition.transform)
            {
                if (!TurnManager.Instance.IsGameStarted)
                {
                    _cardManager.SetupInitialPlayerCard(this.cardIndex);
                }
                else
                {
                    if (!_actionHandler.PlayCardFromHand(true, this.cardIndex))
                    {
                        ReturnToOriginalPosition("카드 배치 실패. AP 부족 또는 슬롯 없음.");
                    }
                }
            }
            else if (_originalParent.TryGetComponent<BenchPos>(out var benchPos))
            {
                if (_currentSlotArea is BattlePos)
                {
                    if (isBenchOnlyUnit)
                    {
                        ReturnToOriginalPosition($"[{fishData.Position}] 유닛은 배틀 필드로 이동할 수 없습니다.");
                        return;
                    }

                    Transform nearestEmptySlot = _currentSlotArea.GetNearestEmptySlot(transform.position);
                    if (nearestEmptySlot != null)
                    {
                        // 기존 벤치 슬롯에서 해제
                        benchPos.ReleaseSlot(_originalParent);

                        // 새로운 배틀 슬롯으로 이동
                        _currentSlotArea.OccupySlot(nearestEmptySlot, gameObject);
                        transform.SetParent(nearestEmptySlot);
                        transform.position = nearestEmptySlot.position;

                        // 이동 후 레이어를 0번으로 변경
                        gameObject.layer = 0;

                        SetTooltipActive(true, false);

                        Debug.Log("벤치에서 배틀필드로 유닛 이동 성공.");
                    }
                    else
                    {
                        // 빈 슬롯이 없으면 복귀
                        ReturnToOriginalPosition("배틀필드에 빈 슬롯이 없습니다.");
                    }
                }
                else
                {
                    // 벤치에서 다른 벤치로 이동은 아직 구현되지 않음
                    ReturnToOriginalPosition("벤치에서 다른 벤치로는 이동할 수 없습니다.");
                }
            }
            else
            {
                // 예상치 못한 위치에서 드래그한 경우
                ReturnToOriginalPosition("유효한 출발점이 아닙니다.");
            }
        }
        else
        {
            // 유효한 슬롯이 아니면 원래 위치로 복귀
            ReturnToOriginalPosition("유효한 슬롯이 아니어서 복귀.");
        }
    }

    private void ReturnToOriginalPosition(string reason)
    {
        // 원래 위치를 찾아 부모를 다시 설정
        Transform parent = _originalParent;
        if (parent == null)
        {
            // 만약 원래 부모가 파괴되었을 경우, 적절한 부모를 찾아 설정
            parent = _cardManager.playerHandPosition.transform;
        }
        transform.SetParent(parent);
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
        if (transform.childCount > 2) // 인덱스 2번(3번째 자식) 확인
        {
            GameObject tooltipObject = transform.GetChild(2).gameObject;
            if (tooltipObject != null)
            {
                tooltipObject.SetActive(isActive);

                if (isActive)
                {
                    if (fishData.IsPlayerCard)
                    {
                        tooltipObject.transform.localRotation = Quaternion.Euler(180, -90, -90);
                    }
                    else
                    {
                        tooltipObject.transform.localRotation = Quaternion.Euler(0, 90, -90);
                    }

                    if (tooltipObject.TryGetComponent<Tooltip>(out var tooltip))
                    {
                        tooltip.SetupTooltip(fishData.Skill_name, fishData.Hp, fishData.Description, fishData.AbilityToAct);

                        if (tooltip._nameText != null)
                        {
                            tooltip._nameText.gameObject.SetActive(isFocus);
                        }
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
            GameObject tooltipObject = transform.GetChild(2).gameObject;

            // 툴팁 컴포넌트를 찾아 HP를 업데이트합니다.
            if (tooltipObject != null && tooltipObject.TryGetComponent<Tooltip>(out var tooltip))
            {
                tooltip.UpdateHpText(currentHp);
            }
        }
    }

    #endregion
}