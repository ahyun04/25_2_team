using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    #region 레퍼런스
    [Header("카드 데이터")]
    public CardData cardData;                         
    public int cardIndex;                             

    [Header("UI")]
    public TextMeshPro nameText;                    
    public TextMeshPro costText;                     

    [Header("상태")]
    public bool isDragging = false;
    private Vector3 originalPosition;                 

    [Header("레이어 마스크")]
    public LayerMask enemyLayer;                      
    public LayerMask playerLayer;                    

    private CardManager _cardManager;                    
    private CardSlotArea _currentSlotArea;
    private MeshRenderer _meshRenderer;
    private Transform currentSlot;

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

        TryGetComponent(out _meshRenderer);

        if (cardData.isPlayerCard)
            gameObject.layer = LayerMask.NameToLayer("Player");
        else
            gameObject.layer = LayerMask.NameToLayer("Enemy");

        SetupCard(cardData);
    }

    // 카드 데이터 설정
    public void SetupCard(CardData data)
    {
        cardData = data;

        if (nameText != null) nameText.text = data.cardName;
        if (costText != null) costText.text = data.manaCost.ToString();
    }

    #endregion

    #region 마우스 클릭/드래그
    private void OnMouseDown()
    {
        // 플레이어 카드일 경우에만 드래그 시작
        if (!cardData.isPlayerCard) return;

        // 드래그 시작 시 기존 슬롯 해제
        if (_currentSlotArea != null && currentSlot != null)
        {
            _currentSlotArea.ReleaseSlot(currentSlot);
            currentSlot = null;
        }

        _currentSlotArea = null;
        originalPosition = transform.position;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (!cardData.isPlayerCard || !isDragging) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);

        if (_meshRenderer != null)
        {
            if (_currentSlotArea != null && TurnManager.Instance.PlayerAP >= cardData.manaCost)
            {
                _meshRenderer.material.color = validColor;
            }
            else if (_currentSlotArea != null && TurnManager.Instance.PlayerAP < cardData.manaCost)
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
        if (!cardData.isPlayerCard || !isDragging) return;

        isDragging = false;

        // 카드 배치 시도
        if (_currentSlotArea != null && _currentSlotArea.IsCardInside)
        {
            if (!TurnManager.Instance.IsGameStarted)
            {
                if (cardData.isPlayerCard)
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
                    transform.DOMove(originalPosition, 0.2f).SetEase(Ease.OutCubic);
                    Debug.Log("카드 배치 실패. AP 부족 또는 슬롯 없음.");
                }
            }
        }
        else
        {
            // 유효한 슬롯이 아니면 원래 위치로 복귀
            transform.DOMove(originalPosition, 0.2f).SetEase(Ease.OutCubic);
        }

        if (_meshRenderer != null)
            _meshRenderer.material.color = normalColor;

        _currentSlotArea = null; // 드롭 후 슬롯 참조 초기화
    }

    #endregion

    #region 공간 트리거 관련
    private void OnTriggerEnter(Collider other)
    {
        if (!isDragging) return; // 드래그 중이 아니면 무시

        CardSlotArea area = other.GetComponent<CardSlotArea>();
        if (area != null)
        {
            // 같은 팀인지 확인
            if ((area.teamLayer.value & (1 << gameObject.layer)) != 0)
            {
                _currentSlotArea = area;
            }
            else
            {
                _currentSlotArea = null; // 팀이 다르면 절대 세팅 안 함
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isDragging) return; // 드래그 중에만 체크

        CardSlotArea area = other.GetComponent<CardSlotArea>();
        if (area != null && (area.teamLayer.value & (1 << gameObject.layer)) != 0)
        {
            _currentSlotArea = area; // 드래그 중에도 안전하게 재확인
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
}