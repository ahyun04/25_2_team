using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandController : MonoBehaviour
{
    #region 레퍼런스
    private UIManager _UIManager;

    [Header("블러")]
    public GameObject _blurBackGround;
    public bool isBluring = false;

    [Header("마우스 클릭 효과")]
    private Vector3 _originalPlayerCardScale;
    public Vector3 OriginalPlayerCardScale => _originalPlayerCardScale;

    [SerializeField] private float _clickedScaleMultiplier = 1.5f;
    [SerializeField] private float _cardFocusScaleMultiplier = 1.2f;
    [SerializeField] private float _scaleAnimDuration = 0.2f;
    private GameObject _currentFocusedCard = null;
    private Vector3 _focusOffset = new Vector3(0, 6f, -2f);
    private Vector3 _lastFocusedCardOriginalPos;
    private bool _isHandExpanded = false;

    #endregion

    #region 초기화, 업데이트
    private void Start()
    {
        _UIManager = FindObjectOfType<UIManager>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    #endregion

    #region 핸드 트리거
    public void HandleMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Case 1: 손패 영역(playerHandPosition)을 클릭했을 때
            if (hit.transform == CardManager.Instance.playerHandPosition)
            {
                SetPlayerHandScale(_clickedScaleMultiplier);
                isBluring = true;
                _blurBackGround.SetActive(true);
                _UIManager.SetAllBattleAndBenchCardTooltips(false);
                _isHandExpanded = true;
                UnfocusCard();
            }
            // Case 2: 손패에 있는 개별 카드(PlayerCard)를 클릭했을 때
            else if (hit.transform.CompareTag("PlayerCard"))
            {
                if (!_blurBackGround.activeSelf) return;

                if (!CardManager.Instance.playerCardObjects.Contains(hit.transform.gameObject)) return;

                if (_currentFocusedCard == hit.transform.gameObject)
                {
                    UnfocusCard();
                }
                else
                {
                    FocusCard(hit.transform.gameObject);
                }
            }
            // Case 3: 손패 영역 외의 다른 곳을 클릭했고, "손패가 확장된 상태일 때만"
            else if (_isHandExpanded)
            {
                // 손패 전체와 배경을 원래대로 되돌립니다.
                SetPlayerHandScaleToOriginal();
                isBluring = false;
                _blurBackGround.SetActive(false);
                _UIManager.SetAllBattleAndBenchCardTooltips(true);
                _isHandExpanded = false;
                UnfocusCard();
            }
        }
        // 아무것도 클릭하지 않았을 때도, "손패가 확장된 상태일 때만"
        else if (_isHandExpanded)
        {
            // 아무것도 클릭하지 않았을 때
            SetPlayerHandScaleToOriginal();
            isBluring = false;
            _blurBackGround.SetActive(false);
            _UIManager.SetAllBattleAndBenchCardTooltips(true);
            _isHandExpanded = false;
            UnfocusCard();
        }
    }

    public bool IsHandExpanded()
    {
        return _isHandExpanded;
    }

    // 외부에서 특정 카드가 포커스된 상태를 확인할 수 있는 public 메서드
    public bool IsCardFocused(GameObject card)
    {
        return _currentFocusedCard == card;
    }

    private void SetPlayerHandScale(float targetScaleMultiplier)
    {
        Vector3 originalScale = _originalPlayerCardScale;
        Vector3 targetScale = originalScale * targetScaleMultiplier;

        foreach (var cardObj in CardManager.Instance.playerCardObjects)
        {
            if (cardObj != null)
            {
                cardObj.transform.DOScale(targetScale, _scaleAnimDuration).SetEase(Ease.OutCubic);
            }
        }
    }

    private void SetPlayerHandScaleToOriginal()
    {
        Vector3 originalScale = _originalPlayerCardScale;

        foreach (var cardObj in CardManager.Instance.playerCardObjects)
        {
            if (cardObj != null)
            {
                cardObj.transform.DOScale(originalScale, _scaleAnimDuration).SetEase(Ease.OutCubic);
            }
        }
    }

    private void FocusCard(GameObject cardToFocus)
    {
        // 기존에 포커스된 카드가 있다면 원래 크기로 되돌립니다.
        if (_currentFocusedCard != null)
        {
            _currentFocusedCard.transform.DOScale(_originalPlayerCardScale * _clickedScaleMultiplier, _scaleAnimDuration);
            _UIManager.SetTooltipForCard(_currentFocusedCard, false, false);

            _currentFocusedCard.transform.DOMove(_lastFocusedCardOriginalPos, _scaleAnimDuration).SetEase(Ease.OutCubic);
        }

        // 포커스된 카드를 제외하고 나머지 손패 카드의 레이어를 Default(0)로 변경
        foreach (var cardObj in CardManager.Instance.playerCardObjects)
        {
            if (cardObj != cardToFocus)
            {
                cardObj.layer = 0; // Default 레이어
            }
        }

        // 손패 외 카드면 무시
        if (!CardManager.Instance.playerCardObjects.Contains(cardToFocus)) return;

        _currentFocusedCard = cardToFocus;

        // 현재 위치 저장
        _lastFocusedCardOriginalPos = cardToFocus.transform.position;

        // 포커스 크기
        Vector3 targetScale = _originalPlayerCardScale * _clickedScaleMultiplier * _cardFocusScaleMultiplier;
        _currentFocusedCard.transform.DOScale(targetScale, _scaleAnimDuration).SetEase(Ease.OutCubic);

        // 포커스 위치 이동
        Vector3 targetPos = _lastFocusedCardOriginalPos + _focusOffset;
        _currentFocusedCard.transform.DOMove(targetPos, _scaleAnimDuration).SetEase(Ease.OutCubic);

        _UIManager.SetTooltipForCard(_currentFocusedCard, true, true);
    }

    private void UnfocusCard()
    {
        if (_currentFocusedCard != null)
        {
            // 크기 복구
            Vector3 originalHandScale = _originalPlayerCardScale * _clickedScaleMultiplier;
            _currentFocusedCard.transform.DOScale(originalHandScale, _scaleAnimDuration).SetEase(Ease.OutCubic);

            // 위치 복구
            _currentFocusedCard.transform.DOMove(_lastFocusedCardOriginalPos, _scaleAnimDuration).SetEase(Ease.OutCubic);

            _UIManager.SetTooltipForCard(_currentFocusedCard, false, false);
            _currentFocusedCard = null;
        }

        // 모든 손패 카드의 레이어를 다시 Render(9)로 복구
        foreach (var cardObj in CardManager.Instance.playerCardObjects)
        {
            if (cardObj != null)
            {
                cardObj.layer = 9; // Render 레이어
            }
        }
    }

    public void SetOriginalCardScale(Vector3 scale)
    {
        // 아직 스케일이 설정되지 않았을 때(값이 0,0,0일 때) 딱 한 번만 값을 저장합니다.
        if (_originalPlayerCardScale == Vector3.zero)
        {
            _originalPlayerCardScale = scale;
        }
    }
    #endregion
}
