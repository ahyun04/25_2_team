using DG.Tweening;
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
    private CardSlotArea _currentSlotArea;
    private MeshRenderer _meshRenderer;
    private Transform _currentSlot;
    private Transform _originalParent;

    [Header("툴팁")]
    [SerializeField] private GameObject tooltipPrefab;

    [Header("색상 설정")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;

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

        TryGetComponent(out _meshRenderer);

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

        if (_meshRenderer != null)
        {
            if (_currentSlotArea != null && TurnManager.Instance.PlayerAP >= fishData.AbilityToAct)
            {
                _meshRenderer.material.color = validColor;
            }
            else if (_currentSlotArea != null && TurnManager.Instance.PlayerAP < fishData.AbilityToAct)
            {
                _meshRenderer.material.color = invalidColor;
            }
            else
            {
                _meshRenderer.material.color = normalColor;
            }
        }
    }

    private void OnMouseUp()
    {
        if (!fishData.IsPlayerCard || !isDragging) return;

        isDragging = false;

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
                    transform.DOMove(_originalPosition, 0.2f).SetEase(Ease.OutCubic);
                    transform.SetParent(_originalParent);
                    Debug.Log("카드 배치 실패. AP 부족 또는 슬롯 없음.");
                }
            }
        }
        else
        {
            // 유효한 슬롯이 아니면 원래 위치로 복귀
            transform.SetParent(_originalParent);
            transform.DOMove(_originalPosition, 0.2f).SetEase(Ease.OutCubic);
        }

        if (_meshRenderer != null)
            _meshRenderer.material.color = normalColor;

        _currentSlotArea = null; 
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

    public void SetTooltipActive(bool isActive)
    {
        SetTooltipActive(isActive, false);
    }

    public void SetTooltipActive(bool isActive, bool isFocus)
    {
        if (transform.childCount > 0)
        {
            GameObject tooltipObject = transform.GetChild(0).gameObject;
            if (tooltipObject != null)
            {
                // 툴팁 오브젝트 전체 활성/비활성화
                tooltipObject.SetActive(isActive);

                // 툴팁이 활성화될 때만 이름 표시 로직 실행
                if (isActive && tooltipObject.TryGetComponent<Tooltip>(out var tooltip))
                {
                    // 툴팁 데이터 업데이트
                    tooltip.SetupTooltip(fishData.Name, fishData.Hp, fishData.Description, fishData.AbilityToAct);

                    if (tooltip._nameText != null)
                    {
                        tooltip._nameText.gameObject.SetActive(isFocus);
                    }
                }
            }
        }
    }

    #endregion
}