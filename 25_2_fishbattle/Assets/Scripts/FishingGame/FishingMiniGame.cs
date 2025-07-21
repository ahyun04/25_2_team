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

        float waitTime = Random.Range(1f, 2f);
        yield return new WaitForSeconds(waitTime);

        Debug.Log("물고기가 찌를 물었다!");
        _isFishing = false;
        _isBobberHit = true;

        // 플레이어 입력을 기다림 (2초 내 입력 없으면 놓침)
        yield return StartCoroutine(WaitForPlayerInput());
    }

    private IEnumerator WaitForPlayerInput()
    {
        float timer = 2f;
        bool inputReceived = false;

        while (timer > 0f)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                inputReceived = true;
                break;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        if (inputReceived)
        {
            SpawnTargetInBar();
            MovingRedBox();
        }
        else
        {
            Debug.Log("놓쳤다..");
            _startFishingButton.gameObject.SetActive(true);
        }

        _isBobberHit = false;
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
        // 미니게임 상태가 아니면 무시
        if (!_isFishing && !_isBobberHit && !_barObj.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 찌를 물고 아직 미니게임 시작 전이면 입력 처리 막기 (WaitForPlayerInput에서 처리하므로)
            if (_isBobberHit) return;

            // 실제 미니게임 중일 때만 판정
            if (_currentRedRect == null || _currentTargetRect == null) return;

            bool isHit = CheckHit();

            if (isHit)
            {
                Debug.Log("성공!");
            }
            else
            {
                Debug.Log("실패!");
            }

            EndMiniGame();
        }
    }

    private void EndMiniGame()
    {
        _barObj.SetActive(false);

        if (_currentRedRect != null)
        {
            Destroy(_currentRedRect.gameObject);
            _currentRedRect = null;
        }

        if (_currentTargetRect != null)
        {
            Destroy(_currentTargetRect.gameObject);
            _currentTargetRect = null;
        }

        _startFishingButton.gameObject.SetActive(true);
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
