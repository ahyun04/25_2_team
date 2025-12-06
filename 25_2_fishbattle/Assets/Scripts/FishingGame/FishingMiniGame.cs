using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishingMiniGame : MonoBehaviour
{
    #region 레퍼런스
    [Header("애니메이션")]
    [SerializeField] private List<Animator> _animators;

    [Header("이펙트 설정 (추가됨)")]
    [SerializeField] private GameObject  _splashPoint;       // 찌가 물에 닿는 위치 (Empty Object 등)
    [SerializeField] private float _castEffectDelay = 1.0f; // 던지기 시작 후 찌가 물에 닿기까지 걸리는 시간

    [Header("코루틴")]
    private Coroutine _fishingCoroutine;

    [Header("카메라")]
    [SerializeField] private Camera _canvasCam;

    [Header("상태 플래그")]
    private bool _isFishing = false;        // 찌가 물고기를 기다리는 중
    private bool _isBobberHit = false;      // 물고기가 찌를 무는 이벤트 발생
    private bool _isReeling = false;        // 릴 감기 미니게임 진행 중
    private bool _isDragging = false;
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
    [SerializeField] private float chance = 1f;                 // 심해 가는 확률 (1f = 100%)

    [Header("미니게임 설정")]
    [SerializeField] private GameObject _miniGameGroup;         // 미니게임 UI 전체 그룹 (바 + 낚시대)
    [SerializeField] private Slider _progressBar;               // 상단 게이지 바

    [Header("핸들 및 커서 설정")]
    [SerializeField] private RectTransform _handleRect;         // 회전하는 전체 낚시대 핸들 몸통
    [SerializeField] private Transform _grabPoint;              
    [SerializeField] private GameObject _virtualHandImage;

    private float _rotationOffset = 0f;
    [SerializeField] private int _targetRotations = 10;         // 목표 회전 수 (10바퀴)
    private float _currentRotationSum = 0f;                     // 현재 누적 회전각
    private float _requiredRotationSum;                         // 목표 누적 회전각 (10 * 360)
    private float _prevAngle = 0f;                              // 이전 프레임의 각도

    [Header("물고기 확인 패널")]
    [SerializeField] private Image _hookAFishPanel;
    [SerializeField] private TextMeshProUGUI _hookAFishNameText;
    [SerializeField] private Button _putInBoxButton;
    [SerializeField] private Transform _fishDisplayPoint;
    [SerializeField] private Button _registerCollectionButton;
    [SerializeField] private TextMeshProUGUI _collectionNoticeText;

    public bool IsResultPanelActive => _hookAFishPanel.gameObject.activeInHierarchy;
    private GameObject _fishPrefab;

    [Header("인벤토리 설정")]
    [SerializeField] private FishDatabaseSO _fishDatabase;
    [SerializeField] private InventoryHolder _playerInventory;
    private FishSO _caughtFishSO;

    #endregion

    #region 초기화
    void Start()
    {
        _playerInventory = FindObjectOfType<InventoryHolder>();

        if (_canvasCam == null)
        {
            _canvasCam = Camera.main;
            Debug.LogWarning("UI Camera가 연결되지 않아 Main Camera를 사용합니다. 정확한 클릭을 위해 Inspector에서 할당해주세요.");
        }

        if (_miniGameGroup != null) _miniGameGroup.SetActive(false);

        _hookAFishPanel.gameObject.SetActive(false);
        _bobberHitText.gameObject.SetActive(false);

        if (_virtualHandImage != null) _virtualHandImage.SetActive(false);

        _requiredRotationSum = _targetRotations * 360f;
    }

    void Update()
    {
        if (_isBobberHit)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartReelGame();
            }
            return;
        }

        if (_isReeling)
        {
            HandleReelInput();
        }
    }

    #endregion

    #region 낚시 흐름
    public void StartFishing()
    {
        if (!MiniGameManager.TryStartMiniGame()) return;

        ResetAllAnimations();
        SetAnimBool("isCast", true);

        if (_fishingCoroutine == null)
        {
            _fishingCoroutine = StartCoroutine(FishingRoutine());
        }
    }

    // 찌가 물고기를 기다리는 시간
    private IEnumerator FishingRoutine()
    {
        yield return null;
        SetAnimBool("isCast", false);

        yield return new WaitForSeconds(_castEffectDelay);
        EffectManager.Instance.PlayEffect("Splash_Water", _splashPoint.transform.position, Quaternion.identity);

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

    private IEnumerator WaitForPlayerInput()
    {
        float timer = 2f;

        while (timer > 0f)
        {
            if (!_isBobberHit) yield break;

            timer -= Time.deltaTime;
            yield return null;
        }

        if (_isBobberHit)
        {
            Debug.Log("놓쳤다..");
            FailFishing();
        }
    }

    private void StartReelGame()
    {
        _isBobberHit = false;
        _bobberHitText.gameObject.SetActive(false);

        SetAnimBool("isCast", false);
        SetAnimBool("isHook", true);

        _isReeling = true;
        _isDragging = false;
        _currentRotationSum = 0f;

        if (_miniGameGroup != null) _miniGameGroup.SetActive(true);
        if (_progressBar != null) _progressBar.value = 0f;

        // 시작할 때 마우스 각도 계산
        Vector2 dir = Input.mousePosition - _handleRect.position;
        _prevAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    // 낚시 실패 처리
    private void FailFishing()
    {
        _bobberHitText.text = "";
        _bobberHitText.gameObject.SetActive(false);
        _isFishing = false;
        _isBobberHit = false;
        _isReeling = false;
        _fishingCoroutine = null;

        MiniGameManager.EndMiniGame();
    }

    #endregion

    #region 릴 감기 로직
    private void HandleReelInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = _canvasCam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.transform == _grabPoint)
            {
                _isDragging = true;

                if (Cursor.visible) Cursor.visible = false;
                if (_virtualHandImage != null) _virtualHandImage.SetActive(true);

                Vector2 dir = Input.mousePosition - _handleRect.position;
                float startMouseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                _rotationOffset = _handleRect.eulerAngles.z - startMouseAngle;
                _prevAngle = startMouseAngle;
            }
        }

        // 드래그 종료
        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            if (!Cursor.visible) Cursor.visible = true;
            if (_virtualHandImage != null) _virtualHandImage.SetActive(false);
        }

        // 회전 계산
        if (_isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 handlePos = _handleRect.position;
            Vector2 direction = mousePos - handlePos;

            float currentMouseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            _handleRect.rotation = Quaternion.Euler(0, 0, currentMouseAngle + _rotationOffset);

            float angleStep = Mathf.DeltaAngle(_prevAngle, currentMouseAngle);

            if (angleStep > 0) _currentRotationSum += angleStep;
            if (_progressBar != null) _progressBar.value = _currentRotationSum / _requiredRotationSum;

            _prevAngle = currentMouseAngle;

            if (_currentRotationSum >= _requiredRotationSum) SuccessFishing();
        }
    }

    private void SuccessFishing()
    {
        // 성공 시 커서 원상복구
        if (!Cursor.visible) Cursor.visible = true;
        if (_virtualHandImage != null) _virtualHandImage.SetActive(false);

        _isReeling = false;
        _isDragging = false;
        _isFishing = false;
        Debug.Log("낚시 성공!");

        SetAnimBool("isHook", false);
        SetAnimBool("isHooked", true);

        if (_miniGameGroup != null) _miniGameGroup.SetActive(false);

        // 심해 씬 이동 로직
        var sceneManager = SceneManager.Instance;
        if (sceneManager != null && sceneManager.GetCurrentSceneName() == sceneManager.OceanGameSceneName)
        {
            if (Random.value <= chance)
            {
                Debug.Log("심해 발견! 심해 씬으로 이동합니다.");
                EndMiniGame();
                sceneManager.LoadScene(sceneManager.DeepOceanGameSceneName);
                return;
            }
        }

        // 물고기 결과창 표시
        ShowFishResult();
    }

    #endregion

    #region 결과 처리 & 인벤토리
    private void ShowFishResult()
    {
        _hookAFishPanel.gameObject.SetActive(true);

        if (_collectionNoticeText != null)
            _collectionNoticeText.gameObject.SetActive(false);

        if (_fishPrefab != null) Destroy(_fishPrefab);

        FishSpawner fishSpawner = GetComponent<FishSpawner>();
        _caughtFishSO = fishSpawner.GetRandomFishByScene();

        if (_caughtFishSO.Prefab != null)
        {
            _fishPrefab = Instantiate(_caughtFishSO.Prefab, _fishDisplayPoint);
            _fishPrefab.transform.localPosition = Vector3.zero;
            _fishPrefab.transform.localRotation = Quaternion.Euler(0f, -90f, 90f);
            _fishPrefab.transform.localScale = new Vector3(5f, 5f, 5f);
        }

        _hookAFishNameText.text = $"{_caughtFishSO.Name} 를(을) 잡았다!";

        _putInBoxButton.onClick.RemoveAllListeners();
        _putInBoxButton.onClick.AddListener(() => PutFishInInventory());

        if (CollectionManager.Instance != null)
        {
            bool isAlreadyCollected = CollectionManager.Instance.IsFishCollected(_caughtFishSO);

            if (isAlreadyCollected)
            {
                CollectionManager.Instance.RegisterFishToCollection(_caughtFishSO);
            }
        }

        UpdateCollectionButtonState();
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

        if (added) Debug.Log($"{_caughtFishSO.Name} 획득 성공");
        else Debug.Log("인벤토리 가득 참");

        _hookAFishPanel.gameObject.SetActive(false);
        EndMiniGame();
    }

    private void UpdateCollectionButtonState()
    {
        if (CollectionManager.Instance == null)
        {
            if (_registerCollectionButton != null) _registerCollectionButton.gameObject.SetActive(false);
            return;
        }

        bool isCollected = CollectionManager.Instance.IsFishCollected(_caughtFishSO);

        if (isCollected)
        {
            if (_registerCollectionButton != null)
                _registerCollectionButton.gameObject.SetActive(false);
        }
        else
        {
            if (_registerCollectionButton != null)
            {
                _registerCollectionButton.gameObject.SetActive(true);
                _registerCollectionButton.onClick.RemoveAllListeners();
                _registerCollectionButton.onClick.AddListener(() => RegisterFishCurrent());
            }
        }
    }

    private void RegisterFishCurrent()
    {
        if (_caughtFishSO == null) return;

        // 도감에 등록 요청
        CollectionManager.Instance.RegisterFishToCollection(_caughtFishSO);

        // 등록 후 버튼을 즉시 숨깁니다.
        if (_registerCollectionButton != null)
            _registerCollectionButton.gameObject.SetActive(false);

        if (_collectionNoticeText != null)
        {
            _collectionNoticeText.gameObject.SetActive(true);
            _collectionNoticeText.text = "도감에 등록 됐습니다!";

            StartCoroutine(HideNoticeRoutine());
        }
    }

    private IEnumerator HideNoticeRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (_collectionNoticeText != null)
        {
            _collectionNoticeText.gameObject.SetActive(false);
        }
    }

    private void EndMiniGame()
    {
        if (!Cursor.visible) Cursor.visible = true;
        if (_virtualHandImage != null) _virtualHandImage.SetActive(false);

        _fishingCoroutine = null;
        _isFishing = false;
        _isReeling = false;
        _isDragging = false;
        if (_miniGameGroup != null) _miniGameGroup.SetActive(false);

        ResetAllAnimations();

        MiniGameManager.EndMiniGame();
    }

    #endregion

    #region 씬 변경 처리
    private void OnDisable()
    {
        if (!Cursor.visible) Cursor.visible = true;

        if (_isFishing || _isReeling)
        {
            _isFishing = false;
            _isReeling = false;

            if (_fishingCoroutine != null)
            {
                StopCoroutine(_fishingCoroutine);
                _fishingCoroutine = null;
            }

            ResetAllAnimations();

            if (MiniGameManager.IsMiniGameRunning)
            {
                MiniGameManager.EndMiniGame();
            }
        }
    }

    #endregion

    #region 애니메이션 헬퍼 함수

    // 모든 애니메이터의 Bool 값을 일괄 설정하는 함수
    private void SetAnimBool(string paramName, bool value)
    {
        if (_animators == null) return;

        foreach (var anim in _animators)
        {
            if (anim != null) anim.SetBool(paramName, value);
        }
    }

    // 모든 애니메이션 상태를 초기화(False)하는 함수
    private void ResetAllAnimations()
    {
        SetAnimBool("isCast", false);
        SetAnimBool("isHook", false);
        SetAnimBool("isHooked", false);
    }

    #endregion
}
