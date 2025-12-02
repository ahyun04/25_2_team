using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManager : SingletonMono<SceneManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => true;

    [Header("Scene Settings")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";
    [SerializeField] private string _lakeGameSceneName = "LakeMiniGameScene";
    [SerializeField] private string _riverGameSceneName = "RiverMiniGameScene";
    [SerializeField] private string _oceanGameSceneName = "OceanMiniGameScene";
    [SerializeField] private string _deepOceanGameSceneName = "DeepOceanMiniGameScene";
    [SerializeField] private string _fishCardGameSceneName = "FishCardGaemScene";
    [SerializeField] private string _mapSelectionSceneName = "MapSelectionScene";
    [SerializeField] private string _enhancementSceneName = "EnhancementScene";
    [SerializeField] private string _loadingSceneName = "Loading";

    public string OceanGameSceneName => _oceanGameSceneName;
    public string DeepOceanGameSceneName => _deepOceanGameSceneName;

    [Header("Loading Settings")]
    [SerializeField] private float _minimumLoadingTime = 2f;
    [SerializeField] private bool useLoadingScreen = true;
    [SerializeField] private bool useTransitionEffect = true;

    [Header("Transition Settings")]
    [SerializeField] private Sprite _transitionSprite; 
    [SerializeField] private float _slideDuration = 1.0f;
    [SerializeField] private Color _transitionColor = Color.white;

    private Dictionary<string, Vector3> _lastPlayerPositions = new Dictionary<string, Vector3>();

    private string currentSceneName;
    private string targetSceneName;
    private bool isLoading = false;

    private GameObject transitionObject;
    private RectTransform transitionImageRect;
    private Canvas transitionCanvas;

    public float LoadingProgress { get; private set; }

    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();

        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // 슬라이드 UI 생성
        if (useTransitionEffect)
        {
            CreateSlideUI();
        }

        Debug.Log("SceneManager 초기화 완료");
    }

    private void Start()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    #endregion

    #region Slide UI 생성 (수정됨)

    private void CreateSlideUI()
    {
        // 1. 캔버스 생성
        transitionObject = new GameObject("TransitionCanvas");
        transitionCanvas = transitionObject.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = transitionObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject imageObj = new GameObject("TransitionImage");
        imageObj.transform.SetParent(transitionObject.transform);

        Image image = imageObj.AddComponent<Image>();
        image.sprite = _transitionSprite;
        image.color = _transitionColor;

        transitionImageRect = imageObj.GetComponent<RectTransform>();

        transitionImageRect.anchorMin = new Vector2(0, 0);
        transitionImageRect.anchorMax = new Vector2(1, 1);
        transitionImageRect.pivot = new Vector2(0.5f, 0.5f);

        UpdateImagePosition(Screen.width);

        DontDestroyOnLoad(transitionObject);
    }

    // 화면 크기 갱신에 대응하기 위해 위치 설정 함수 분리
    private void UpdateImagePosition(float xOffset)
    {
        if (transitionImageRect == null) return;

        transitionImageRect.offsetMin = new Vector2(xOffset, 0);
        transitionImageRect.offsetMax = new Vector2(xOffset, 0);
    }

    #endregion

    #region 씬 로딩
    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning("이미 씬 로딩 중입니다.");
            return;
        }

        TrySavePlayerPosition();
        targetSceneName = sceneName;

        if (useTransitionEffect)
        {
            StartCoroutine(LoadSceneWithSlide(sceneName));
        }
        else
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }

    public void LoadSceneWithLoadingScreen(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning("이미 씬 로딩 중입니다.");
            return;
        }

        TrySavePlayerPosition();
        targetSceneName = sceneName;
        StartCoroutine(LoadSceneWithLoading(sceneName));
    }

    // 슬라이드 효과와 함께 씬 로딩
    private IEnumerator LoadSceneWithSlide(string sceneName)
    {
        isLoading = true;
        GameManager.Instance?.ChangeGameState(GameState.Loading);

        // 오른쪽 -> 중앙으로 이동 (화면 덮기)
        yield return StartCoroutine(SlideIn());

        // 씬 로딩 (비동기)
        yield return StartCoroutine(LoadSceneAsync(sceneName));

        // 중앙 -> 왼쪽으로 이동 (화면 열기)
        yield return StartCoroutine(SlideOut());

        isLoading = false;
    }

    // 로딩 화면 로직
    private IEnumerator LoadSceneWithLoading(string sceneName)
    {
        isLoading = true;
        GameManager.Instance?.ChangeGameState(GameState.Loading);

        if (useLoadingScreen && !string.IsNullOrEmpty(_loadingSceneName))
        {
            yield return StartCoroutine(LoadSceneAsync(_loadingSceneName));

            float startTime = Time.time;
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                LoadingProgress = asyncLoad.progress;
                if (asyncLoad.progress >= 0.9f)
                {
                    LoadingProgress = 1f;
                    if (Time.time - startTime >= _minimumLoadingTime)
                    {
                        asyncLoad.allowSceneActivation = true;
                    }
                }
                yield return null;
            }
        }
        else
        {
            yield return StartCoroutine(LoadSceneAsync(sceneName));
        }
        isLoading = false;
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            LoadingProgress = asyncLoad.progress;
            yield return null;
        }
        LoadingProgress = 1f;
    }

    #endregion

    #region 슬라이드 이펙트

    // 화면 오른쪽 밖 -> 화면 중앙
    private IEnumerator SlideIn()
    {
        if (transitionImageRect == null) yield break;

        // 활성화
        transitionObject.SetActive(true);

        float timer = 0f;
        float screenWidth = GetCanvasWidth(); // 캔버스 기준 너비

        // 시작 위치: 오른쪽 밖
        Vector2 startPos = new Vector2(screenWidth, 0);
        // 목표 위치: 중앙
        Vector2 targetPos = Vector2.zero;

        // 앵커 오프셋 초기화
        transitionImageRect.offsetMin = new Vector2(screenWidth, 0);
        transitionImageRect.offsetMax = new Vector2(screenWidth, 0);

        while (timer < _slideDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / _slideDuration;

            t = Mathf.SmoothStep(0, 1, t);

            float currentX = Mathf.Lerp(screenWidth, 0, t);

            transitionImageRect.offsetMin = new Vector2(currentX, 0);
            transitionImageRect.offsetMax = new Vector2(currentX, 0);

            yield return null;
        }

        // 확실하게 중앙에 고정
        transitionImageRect.offsetMin = Vector2.zero;
        transitionImageRect.offsetMax = Vector2.zero;
    }

    // 화면 중앙 -> 화면 왼쪽 밖
    private IEnumerator SlideOut()
    {
        if (transitionImageRect == null) yield break;

        float timer = 0f;
        float screenWidth = GetCanvasWidth();


        while (timer < _slideDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / _slideDuration;
            t = Mathf.SmoothStep(0, 1, t);

            float currentX = Mathf.Lerp(0, -screenWidth, t);

            transitionImageRect.offsetMin = new Vector2(currentX, 0);
            transitionImageRect.offsetMax = new Vector2(currentX, 0);

            yield return null;
        }

        UpdateImagePosition(screenWidth);
    }

    private float GetCanvasWidth()
    {
        RectTransform canvasRect = transitionCanvas.GetComponent<RectTransform>();
        return canvasRect.rect.width;
    }

    #endregion

    #region Quick Load Methods

    public void LoadMainMenu() => LoadScene(_mainMenuSceneName);
    public void LoadMapSelectionSceneName() => LoadScene(_mapSelectionSceneName);

    public void LoadGameScene()
    {
        LoadScene(_lakeGameSceneName);
        LoadScene(_riverGameSceneName);
        LoadScene(_oceanGameSceneName);
        LoadScene(_deepOceanGameSceneName);
        LoadScene(_fishCardGameSceneName);
        LoadScene(_enhancementSceneName);
    }

    public void ReloadCurrentScene() => LoadScene(currentSceneName);

    public void LoadNextScene()
    {
        int currentIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int nextIndex = (currentIndex + 1) % UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        LoadScene(UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(nextIndex).name);
    }

    public void LoadPreviousScene()
    {
        int currentIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int prevIndex = currentIndex - 1;
        if (prevIndex < 0) prevIndex = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings - 1;
        LoadScene(UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(prevIndex).name);
    }

    #endregion

    #region Scene Events
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        LoadingProgress = 0f;
        if (Time.timeScale == 0f) Time.timeScale = 1f;
        GameEvents.SceneChanged(currentSceneName);
        Debug.Log($"씬 로딩 완료: {scene.name}");
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"씬 언로드 완료: {scene.name}");
    }
    #endregion

    #region 플레이어 위치 저장
    public void SavePlayerPosition(Vector3 position)
    {
        string sceneName = GetCurrentSceneName();
        if (string.IsNullOrEmpty(sceneName)) return;
        _lastPlayerPositions[sceneName] = position;
    }

    public Vector3? GetAndClearSavedPosition()
    {
        string sceneName = GetCurrentSceneName();
        if (string.IsNullOrEmpty(sceneName)) return null;
        if (_lastPlayerPositions.TryGetValue(sceneName, out Vector3 position))
        {
            _lastPlayerPositions.Remove(sceneName);
            return position;
        }
        return null;
    }

    private void TrySavePlayerPosition()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) SavePlayerPosition(player.transform.position);
    }
    #endregion

    #region 유틸리티
    public string GetCurrentSceneName() => currentSceneName;
    public bool IsLoading() => isLoading;
    public bool IsSceneLoaded(string sceneName) => currentSceneName == sceneName;

    public void SetTransitionColor(Color color)
    {
        _transitionColor = color;
        if (transitionObject != null)
        {
            Image img = transitionObject.GetComponentInChildren<Image>();
            if (img != null) img.color = color;
        }
    }
    #endregion
}