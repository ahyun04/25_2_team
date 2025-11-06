using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishingMiniGame : MonoBehaviour
{
    #region 레퍼런스
    [Header("코루틴")]
    private Coroutine _fishingCoroutine;

    [Header("상태 플래그")]
    private bool _isFishing = false;        // 찌가 물고기를 기다리는 중
    private bool _isBobberHit = false;      // 물고기가 찌를 무는 이벤트 발생
    public bool IsFishing
    {
        get => _isFishing;
        set => _isFishing = value;
    }

    public bool IsMiniGameRunning => _fishingCoroutine != null;

    [Header("낚시 텍스트")]
    [SerializeField] private TextMeshProUGUI _bobberHitText;

    [Header("미니게임 랜덤 최소/최대 값")]
    [SerializeField] private float _minWaitTime = 5f;           // 최소 대기 시간
    [SerializeField] private float _maxWaitTime = 15f;          // 최대 대기 시간

    [Header("박스 정보")]
    private RectTransform _currentRedRect;     // 움직이는 빨간 박스
    private RectTransform _currentTargetRect;  // 고정된 노란 타겟 박스

    [Header("낚시 바 설정")]
    [SerializeField] private GameObject _barObj;
    [SerializeField] private List<RectTransform> _targetPositions; // 노란 타겟이 스폰될 위치들
    [SerializeField] private GameObject _targetBoxPrefab;

    [Header("빨간 박스")]
    [SerializeField] private GameObject _redBoxPrefab;
    [SerializeField] private float _height = 100f;                              // 빨간 화살표 높이  
    [SerializeField, Range(10f, 1000f)] private float _moveSpeed = 300f;        // 빨간 화살표 속도 조절

    [Header("물고기 확인 패널")]
    [SerializeField] private Image _hookAFishPanel;
    [SerializeField] private TextMeshProUGUI _hookAFishNameText;
    [SerializeField] private Button _putInBoxButton;
    [SerializeField] private Transform _fishDisplayPoint;

    public bool IsResultPanelActive => _hookAFishPanel.gameObject.activeInHierarchy;
    private GameObject _fishPrefab;

    [Header("기즈모 설정")]
    [SerializeField] private bool _drawGizmoLine = false;
    [SerializeField] private float _gizmoLineLength = 200f;

    [Header("인벤토리 설정")]
    [SerializeField] private FishDatabaseSO _fishDatabase;
    [SerializeField] private InventoryHolder _playerInventory;
    private FishSO _caughtFishSO;

    #endregion

    #region 초기화
    void Start()
    {
        _playerInventory = FindObjectOfType<InventoryHolder>();
        _barObj.SetActive(false);
        _hookAFishPanel.gameObject.SetActive(false);
    }

    void Update()
    {
        Handle(); // 스페이스 입력 처리
    }

    #endregion

    #region 낚시 흐름
    public void StartFishing()
    {
        if (!MiniGameManager.TryStartMiniGame()) return;

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

        // 랜덤 시간 대기
        float waitTime = Random.Range(_minWaitTime, _maxWaitTime);
        yield return new WaitForSeconds(waitTime);

        _bobberHitText.gameObject.SetActive(true);
        _bobberHitText.text = "물고기가 찌를 물었다!";
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
            _bobberHitText.gameObject.SetActive(false);
            SpawnTargetInBar();   // 노란 박스 생성
            MovingRedBox();       // 빨간 박스 움직임 시작
        }
        else
        {
            Debug.Log("놓쳤다..");
            _bobberHitText.text = "";
            _bobberHitText.gameObject.SetActive(false);
            _isFishing = false;
            _fishingCoroutine = null;

            MiniGameManager.EndMiniGame();
        }

        _isBobberHit = false;
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

        _currentRedRect.anchoredPosition = new Vector2(minX, _height);
        _currentRedRect.DOAnchorPosX(maxX, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(false);
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
                _isFishing = false;

                Debug.Log("성공!");
                _hookAFishPanel.gameObject.SetActive(true);

                if (_fishPrefab != null)
                {
                    Destroy(_fishPrefab);
                }

                FishSpawner fishSpawner = GetComponent<FishSpawner>();
                _caughtFishSO = fishSpawner.GetRandomFishByScene();

                if (_caughtFishSO.Prefab != null)
                {
                    _fishPrefab = Instantiate(_caughtFishSO.Prefab, _fishDisplayPoint);
                    _fishPrefab.transform.localPosition = Vector3.zero;

                    if (_caughtFishSO.Prefab.name == "Fish_Flatfish")
                    {
                        _fishPrefab.transform.localRotation = Quaternion.Euler(0f, -180f, 180f);
                    }
                    else
                    {
                        _fishPrefab.transform.localRotation = Quaternion.Euler(0f, -90f, 180f);
                    }

                    _fishPrefab.transform.localScale = new Vector3(50f, 50f, 50f);
                }

                _hookAFishNameText.text = $"{_caughtFishSO.Name} 를(을) 잡았다!";

                _putInBoxButton.onClick.RemoveAllListeners(); 
                _putInBoxButton.onClick.AddListener(() => PutFishInInventory());

                _barObj.SetActive(false);
                _bobberHitText.text = "";
                if (_currentRedRect != null) { Destroy(_currentRedRect.gameObject); _currentRedRect = null; }
                if (_currentTargetRect != null) { Destroy(_currentTargetRect.gameObject); _currentTargetRect = null; }
            }
            else
            {
                Debug.Log("실패!");
                EndMiniGame();
            }
        }
    }

    private void PutFishInInventory()
    {
        if (_caughtFishSO == null) return;

        var inventorySystem = _playerInventory.InventorySystem;
        bool added = false;

        foreach (var slot in inventorySystem.InventorySlots)
        {
            if (slot.ItemData == _caughtFishSO && slot.StackSize < _caughtFishSO.MaxStackSize)
            {
                slot.AddToStack(1);
                inventorySystem.OnInventorySlotChanged?.Invoke(slot);
                added = true;
                break;
            }
        }

        if (!added)
        {
            for (int i = 0; i < inventorySystem.InventorySlots.Count; i++)
            {
                var slot = inventorySystem.InventorySlots[i];

                if (slot.ItemData == null)
                {
                    inventorySystem.InventorySlots[i] = new InventorySlot(_caughtFishSO, 1);
                    inventorySystem.OnInventorySlotChanged?.Invoke(inventorySystem.InventorySlots[i]);
                    added = true;
                    break;
                }
            }
        }

        if (added)
        {
            Debug.Log($"{_caughtFishSO.Name}를 인벤토리에 추가했습니다!");
        }
        else
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
        }

        _hookAFishPanel.gameObject.SetActive(false);
        _fishingCoroutine = null;
        MiniGameManager.EndMiniGame();
    }

    // 미니게임 종료 처리
    private void EndMiniGame()
    {
        _barObj.SetActive(false);
        _bobberHitText.text = "";

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

        _fishingCoroutine = null;

        MiniGameManager.EndMiniGame();
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

        // RectTransform의 월드 좌표를 가져옴
        Vector3 redWorldCenter = _currentRedRect.position;

        // 이 선의 시작점과 끝점을 계산 (높이를 조금 더 낮춰서 선이 보이게 함)
        Vector3 redLineStart = redWorldCenter;
        Vector3 redLineEnd = redWorldCenter + Vector3.down * _gizmoLineLength;

        // 충돌 판정
        bool isHit = CheckHit();

        // 판정 결과에 따라 기즈모 색상 변경
        Gizmos.color = isHit ? Color.red : Color.green;
        Gizmos.DrawLine(redLineStart, redLineEnd);

        // 디버깅을 위해 노란 타겟 박스의 월드 좌표도 시각화
        Vector3 yellowWorldCenter = _currentTargetRect.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(yellowWorldCenter, new Vector3(_currentTargetRect.rect.width, _currentTargetRect.rect.height, 0));
    }

    // 충돌 판정 (빨간선이 노란 박스를 통과하고 있는지)
    private bool CheckHit()
    {
        if (_currentRedRect == null || _currentTargetRect == null) return false;

        // RectTransform의 월드 좌표와 크기를 사용
        Rect redRectWorld = RectTransformToWorldSpace(_currentRedRect);
        Rect yellowRectWorld = RectTransformToWorldSpace(_currentTargetRect);

        // x축 오버랩만 확인
        return redRectWorld.xMin < yellowRectWorld.xMax && redRectWorld.xMax > yellowRectWorld.xMin;
    }

    // RectTransform을 월드 좌표계의 Rect로 변환하는 헬퍼 함수
    private Rect RectTransformToWorldSpace(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        // 왼쪽 아래 코너
        Vector3 bottomLeft = corners[0];
        // 오른쪽 위 코너
        Vector3 topRight = corners[2];

        return new Rect(
            bottomLeft.x,
            bottomLeft.y,
            topRight.x - bottomLeft.x,
            topRight.y - bottomLeft.y
        );
    }

    #endregion

    #region 씬 변경 처리
    private void OnDisable()
    {
        // 만약 일반 낚시 코루틴이 여전히 실행 중이었다면
        if (_fishingCoroutine != null)
        {
            Debug.Log("FishingMiniGame이 비활성화/파괴되어 미니게임을 강제 종료합니다.");
            // 상태 플래그와 매니저 플래그 리셋
            _fishingCoroutine = null;
            MiniGameManager.EndMiniGame();
        }
    }

    #endregion
}
