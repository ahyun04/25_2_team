using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FishingMiniGame : MonoBehaviour
{
    #region 레퍼런스
    [Header("Button")]
    [SerializeField] private Button _startFishingButton;

    [Header("Coroutine")]
    private Coroutine _fishingCoroutine;

    [Header("Checking")]
    private bool _isFishing = false;
    private bool _isBobberHit = false;

    [Header("TestBox")]
    private RectTransform _currentRedRect;
    private RectTransform _currentTargetRect;

    [Header("FishingBar")]
    [SerializeField] private GameObject _barObj;
    [SerializeField] private List<RectTransform> _targetPositions;
    [SerializeField] private GameObject _targetBoxPrefab;

    [Header("RedIndicator")]
    [SerializeField] private GameObject _redBoxPrefab;
    [SerializeField, Range(10f, 1000f)] private float _moveSpeed = 300f;

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmoLine = false;
    [SerializeField] private float gizmoLineLength = 200f;

    #endregion

    #region 초기화
    void Start()
    {
        _startFishingButton.onClick.AddListener(StartFishing);

        _barObj.SetActive(false);
    }

    void Update()
    {
        Handle();
    }

    #endregion

    #region 낚시 미니게임
    private void StartFishing()
    {
        _startFishingButton.gameObject.SetActive(false);

        if (_fishingCoroutine == null)
        {
            _fishingCoroutine = StartCoroutine(FishingRoutine());
        }
    }

    private IEnumerator FishingRoutine()
    {
        _isFishing = true;
        Debug.Log("낚시 시작... 물고기를 기다리는 중...");

        float waitTime = Random.Range(5f, 15f);
        yield return new WaitForSeconds(waitTime);

        Debug.Log("물고기가 찌를 물었다!");
        _isFishing = false;
        _isBobberHit = true;
        
        SpawnTargetInBar();
        MovingRedBox();

        _fishingCoroutine = null;
    }

    private void SpawnTargetInBar()
    {
        if (_targetPositions == null || _targetPositions.Count == 0) return;

        _barObj.SetActive(true);

        int randomIndex = Random.Range(0, _targetPositions.Count);
        RectTransform targetParent = _targetPositions[randomIndex];

        GameObject targetBox = Instantiate(_targetBoxPrefab, targetParent);
        _currentTargetRect = targetBox.GetComponent<RectTransform>();
    }

    private void MovingRedBox()
    {
        GameObject redBox = Instantiate(_redBoxPrefab, _barObj.transform);
        _currentRedRect = redBox.GetComponent<RectTransform>();

        RectTransform barRect = _barObj.GetComponent<RectTransform>();

        float barWidth = barRect.rect.width;
        float redWidth = _currentRedRect.rect.width;

        float minX = -((barWidth - redWidth) * 0.5f);
        float maxX = ((barWidth - redWidth) * 0.5f);
        float distance = Mathf.Abs(maxX - minX);

        float duration = distance / _moveSpeed;

        _currentRedRect.anchoredPosition = new Vector2(minX, 400f);
        _currentRedRect.DOAnchorPosX(maxX, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    #endregion

    #region 컨트롤
    private void Handle()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool isHit = CheckHit();

            if (isHit)
            {
                Debug.Log("성공!");
            }
            else
            {
                Debug.Log("실패!");
            }
        }
    }

    #endregion

    #region 기즈모
    public void DrawGizmosLine(bool isOn)
    {
        drawGizmoLine = isOn;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmoLine || _currentRedRect == null || _currentTargetRect == null) return;

        Vector3 redCenter = _currentRedRect.position;
        Vector3 lineEnd = redCenter + Vector3.down * gizmoLineLength;

        bool isHit = CheckHit();

        Gizmos.color = isHit ? Color.red : Color.green;
        Gizmos.DrawLine(redCenter, lineEnd);
    }

    private bool CheckHit()
    {
        if (_currentRedRect == null || _currentTargetRect == null) return false;

        Vector3 redCenter = _currentRedRect.position;
        Rect yellowRect = new Rect(
            _currentTargetRect.position.x - _currentTargetRect.rect.width * 0.5f,
            _currentTargetRect.position.y - _currentTargetRect.rect.height * 0.5f,
            _currentTargetRect.rect.width,
            _currentTargetRect.rect.height
        );

        return yellowRect.Contains(new Vector2(redCenter.x, yellowRect.center.y));
    }

    #endregion
}
