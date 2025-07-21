using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FishingMiniGame : MonoBehaviour
{
    #region UI 요소
    [Header("낚시 시작 버튼")]
    [SerializeField] private Button _startFishingButton;

    #endregion

    #region 상태 관리
    [Header("코루틴")]
    private Coroutine _fishingCoroutine;

    [Header("상태 플래그")]
    private bool _isFishing = false;        // 찌가 물고기를 기다리는 중
    private bool _isBobberHit = false;      // 물고기가 찌를 무는 이벤트 발생

    #endregion

    #region 미니게임 박스 (노란/빨간 박스)
    [Header("박스 정보")]
    private RectTransform _currentRedRect;     // 움직이는 빨간 박스
    private RectTransform _currentTargetRect;  // 고정된 노란 타겟 박스

    #endregion

    #region 낚시 미니게임 바
    [Header("낚시 바 설정")]
    [SerializeField] private GameObject _barObj;
    [SerializeField] private List<RectTransform> _targetPositions; // 노란 타겟이 스폰될 위치들
    [SerializeField] private GameObject _targetBoxPrefab;

    #endregion

    #region 빨간 바 움직임
    [Header("빨간 박스")]
    [SerializeField] private GameObject _redBoxPrefab;
    [SerializeField, Range(10f, 1000f)] private float _moveSpeed = 300f; // 낮을수록 빠름

    #endregion

    #region 디버그 기즈모 (선 표시)
    [Header("기즈모 설정")]
    [SerializeField] private bool _drawGizmoLine = false;
    [SerializeField] private float _gizmoLineLength = 200f;

    #endregion

    #region 초기화
    void Start()
    {
        _startFishingButton.onClick.AddListener(StartFishing);
        _barObj.SetActive(false); // 시작 시 바는 비활성화
    }

    void Update()
    {
        Handle(); // 스페이스 입력 처리
    }

    #endregion

    #region 낚시 흐름
    private void StartFishing()
    {
        _startFishingButton.gameObject.SetActive(false); // 버튼 숨기기

        if (_fishingCoroutine == null)
        {
            _fishingCoroutine = StartCoroutine(FishingRoutine());
        }
    }

    // 찌가 물고기를 기다리는 시간
    private IEnumerator FishingRoutine()
    {
        _isFishing = true;
        Debug.Log("낚시 시작... 물고기를 기다리는 중...");

        float waitTime = Random.Range(5f, 15f);
        yield return new WaitForSeconds(waitTime);

        Debug.Log("물고기가 찌를 물었다!");
        _isFishing = false;
        _isBobberHit = true;

        yield return StartCoroutine(WaitForPlayerInput());
    }

    // 2초 안에 스페이스바 입력 기다림
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
            SpawnTargetInBar();   // 노란 박스 생성
            MovingRedBox();       // 빨간 박스 움직임 시작
        }
        else
        {
            Debug.Log("놓쳤다..");
            _startFishingButton.gameObject.SetActive(true);
        }

        _isBobberHit = false;
        _fishingCoroutine = null;
    }

    #endregion

    #region 타겟 & 인디케이터 생성
    // 노란 박스를 특정 위치에 랜덤 생성
    private void SpawnTargetInBar()
    {
        if (_targetPositions == null || _targetPositions.Count == 0) return;

        _barObj.SetActive(true);

        int randomIndex = Random.Range(0, _targetPositions.Count);
        RectTransform targetParent = _targetPositions[randomIndex];

        GameObject targetBox = Instantiate(_targetBoxPrefab, targetParent);
        _currentTargetRect = targetBox.GetComponent<RectTransform>();
    }

    // 빨간 박스를 좌우로 이동시킴
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

    #region 입력 판정 & 종료 처리
    private void Handle()
    {
        // 미니게임이 진행 중이 아닐 때는 입력 무시
        if (!_isFishing && !_isBobberHit && !_barObj.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 찌 반응 상태에서는 입력 처리 X (2초 안에만 판단됨)
            if (_isBobberHit) return;

            // 미니게임 시작된 후만 판정 가능
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

    // 미니게임 종료 처리
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
    // 외부에서 기즈모 On/Off
    public void DrawGizmosLine(bool isOn)
    {
        _drawGizmoLine = isOn;
    }

    // Scene에서 디버그 선 표시
    private void OnDrawGizmos()
    {
        if (!_drawGizmoLine || _currentRedRect == null || _currentTargetRect == null) return;

        Vector3 redCenter = _currentRedRect.position;
        Vector3 lineEnd = redCenter + Vector3.down * _gizmoLineLength;

        bool isHit = CheckHit();

        Gizmos.color = isHit ? Color.red : Color.green;
        Gizmos.DrawLine(redCenter, lineEnd);
    }

    // 충돌 판정 (빨간선이 노란 박스를 통과하고 있는지)
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
