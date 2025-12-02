using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneManager : SingletonMono<SceneManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => true;
    //public static SceneManager Instance;


    [Header("Scene Settings")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";
    [SerializeField] private string _lakeGameSceneName = "LakeMiniGameScene";
    [SerializeField] private string _riverGameSceneName = "RiverMiniGameScene";
    [SerializeField] private string _oceanGameSceneName = "OceanMiniGameScene";
    [SerializeField] private string _deepOceanGameSceneName = "DeepOceanMiniGameScene";
    [SerializeField] private string _fishCardGameSceneName = "FishCardGaemScene";
    [SerializeField] private string _mapSelectionSceneName = "MapSelectionScene";
    [SerializeField] private string _enhancementSceneName = "EnhancementScene";
    [SerializeField] private string _loadingSceneName = "LoadingScene";

    public string OceanGameSceneName => _oceanGameSceneName;
    public string DeepOceanGameSceneName => _deepOceanGameSceneName;

    [Header("Loading Settings")]
    [SerializeField] private float _minimumLoadingTime = 2f;
    [SerializeField] private bool useLoadingScreen = true;
    [SerializeField] private bool useFadeEffect = true;
    [Header("Loading UI")]
    public GameObject loadingScreen;
    public Slider progressBar;

    [Header("Fade Settings")]
    [SerializeField] private float _fadeSpeed = 1f;
    [SerializeField] private Color _fadeColor = Color.black;

    private Dictionary<string, Vector3> _lastPlayerPositions = new Dictionary<string, Vector3>();

    private string currentSceneName;
    private string targetSceneName;
    private bool isLoading = false;
    private CanvasGroup fadeCanvasGroup;
    private GameObject fadeObject;

    // 씬 로딩 진행률 추적
    public float LoadingProgress { get; private set; }

    #endregion

    #region 초기화
    protected override void Awake()
    {
        if (Instance == null)
        {
            //Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        base.Awake();

        // 현재 씬 이름 저장
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // 페이드 UI 생성
        if (useFadeEffect)
        {
            CreateFadeUI();
        }

        Debug.Log("SceneManager 초기화 완료");
    }

    private void Start()
    {
        // 씬 로딩 이벤트 구독
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    #endregion

    #region Fade UI 생성

    private void CreateFadeUI()
    {
        // 페이드용 Canvas 생성
        fadeObject = new GameObject("FadeCanvas");
        //_fadeObject.transform.SetParent(transform);

        Canvas canvas = fadeObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // 가장 위에 렌더링

        // CanvasGroup 추가
        fadeCanvasGroup = fadeObject.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;

        // 페이드 이미지 생성
        GameObject fadeImage = new GameObject("FadeImage");
        fadeImage.transform.SetParent(fadeObject.transform);

        UnityEngine.UI.Image image = fadeImage.AddComponent<UnityEngine.UI.Image>();
        image.color = _fadeColor;

        // 전체 화면 크기로 설정
        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        DontDestroyOnLoad(fadeObject);
    }

    #endregion

    #region 씬 로딩
    // 기본 씬 로딩 (로딩 화면 없음)
    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning("이미 씬 로딩 중입니다.");
            return;
        }

        TrySavePlayerPosition();

        targetSceneName = sceneName;

        if (useFadeEffect)
        {
            StartCoroutine(LoadSceneWithFade(sceneName));
        }
        else
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }

    // 로딩 화면을 사용한 씬 로딩
    public void LoadSceneWithLoadingScreen(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning("이미 씬 로딩 중입니다.");
            return;
        }

        TrySavePlayerPosition();

        targetSceneName = sceneName;
        StartCoroutine(LoadAsync(sceneName));
    }

    // 페이드 효과와 함께 씬 로딩
    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        isLoading = true;
        GameManager.Instance?.ChangeGameState(GameState.Loading);

        // 페이드 인
        //yield return StartCoroutine(FadeIn());

        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("LoadingScene");

        // 씬 로딩
        yield return StartCoroutine(LoadSceneAsync(sceneName));



        // 페이드 아웃
        //yield return StartCoroutine(FadeOut());

        isLoading = false;
    }

    // 로딩 화면을 사용한 씬 로딩
    private IEnumerator LoadSceneWithLoading(string sceneName)
    {
        isLoading = true;
        GameManager.Instance?.ChangeGameState(GameState.Loading);

        // 로딩 화면 로드
        if (useLoadingScreen && !string.IsNullOrEmpty(_loadingSceneName))
        {
            yield return StartCoroutine(LoadSceneAsync(_loadingSceneName));

            // 최소 로딩 시간 대기
            float startTime = Time.time;

            // 실제 씬 로딩
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                LoadingProgress = asyncLoad.progress;

                // 로딩이 90% 완료되면 대기
                if (asyncLoad.progress >= 0.9f)
                {
                    LoadingProgress = 1f;

                    // 최소 로딩 시간 체크
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
            // 로딩 화면 없이 직접 로딩
            yield return StartCoroutine(LoadSceneAsync(sceneName));
        }

        isLoading = false;
    }

    // 비동기 씬 로딩
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        //StartCoroutine(LoadAsync(sceneName));
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            LoadingProgress = asyncLoad.progress;
            yield return null;
        }

        LoadingProgress = 1f;
    }

    IEnumerator LoadAsync(string sceneName)
    {
        loadingScreen.SetActive(true);
        progressBar.value = 0f;

        AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            progressBar.value = Mathf.Clamp01(op.progress / 0.9f);
            yield return null;
        }

        progressBar.value = 1f;
        yield return new WaitForSeconds(0.2f);

        op.allowSceneActivation = true;
        loadingScreen.SetActive(false);
    }

    #endregion

    #region 페이드 이펙트

    /*private IEnumerator FadeIn()
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true;

        while (fadeCanvasGroup.alpha < 1f)
        {
            fadeCanvasGroup.alpha += _fadeSpeed * Time.unscaledDeltaTime;
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        if (fadeCanvasGroup == null) yield break;

        while (fadeCanvasGroup.alpha > 0f)
        {
            fadeCanvasGroup.alpha -= _fadeSpeed * Time.unscaledDeltaTime;
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }*/

    #endregion

    #region Quick Load Methods

    public void LoadMainMenu()
    {
        LoadScene(_mainMenuSceneName);
    }

    public void LoadMapSelectionSceneName()
    {
        LoadScene(_mapSelectionSceneName);
    }

    public void LoadGameScene()
    {
        LoadScene(_lakeGameSceneName);
        LoadScene(_riverGameSceneName);
        LoadScene(_oceanGameSceneName);
        LoadScene(_deepOceanGameSceneName);
        LoadScene(_fishCardGameSceneName);
        LoadScene(_enhancementSceneName);
    }

    public void ReloadCurrentScene()
    {
        LoadScene(currentSceneName);
    }

    public void LoadNextScene()
    {
        int currentIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int nextIndex = (currentIndex + 1) % UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;

        UnityEngine.SceneManagement.SceneManager.LoadScene(nextIndex);
    }

    public void LoadPreviousScene()
    {
        int currentIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int prevIndex = currentIndex - 1;

        if (prevIndex < 0)
            prevIndex = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings - 1;

        UnityEngine.SceneManagement.SceneManager.LoadScene(prevIndex);
    }

    #endregion

    #region Scene Events

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        LoadingProgress = 0f;

        if (Time.timeScale == 0f)
        {
            Debug.LogWarning("Time.timeScale이 0인 상태로 씬이 로드되어 1로 복구합니다.");
            Time.timeScale = 1f;
        }

        // SceneConfig가 GameState를 처리하도록 이벤트만 발생
        GameEvents.SceneChanged(currentSceneName);

        Debug.Log($"씬 로딩 완료: {scene.name}");
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"씬 언로드 완료: {scene.name}");
    }

    #endregion

    #region 플레이어 위치 저장/불러오기
    // 현재 씬의 플레이어 위치를 딕셔너리에 저장합니다.
    public void SavePlayerPosition(Vector3 position)
    {
        string sceneName = GetCurrentSceneName();
        if (string.IsNullOrEmpty(sceneName)) return;

        _lastPlayerPositions[sceneName] = position;
        Debug.Log($"[{sceneName}] 씬의 플레이어 위치 저장: {position}");
    }

    // 현재 씬의 저장된 위치를 불러오고, 딕셔너리에서 삭제합니다.
    // 저장된 위치가 있으면 Vector3, 없으면 null
    public Vector3? GetAndClearSavedPosition()
    {
        string sceneName = GetCurrentSceneName();
        if (string.IsNullOrEmpty(sceneName)) return null;

        if (_lastPlayerPositions.TryGetValue(sceneName, out Vector3 position))
        {
            Debug.Log($"[{sceneName}] 씬의 저장된 위치 불러오기: {position}");
            _lastPlayerPositions.Remove(sceneName);
            return position;
        }

        return null; // 저장된 위치 없음
    }

    // 씬을 떠나기 직전, 현재 씬의 플레이어 위치를 저장하려고 시도합니다.
    private void TrySavePlayerPosition()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
            SavePlayerPosition(player.transform.position);
    }
    #endregion

    #region 유틸리티

    public string GetCurrentSceneName()
    {
        return currentSceneName;
    }

    public bool IsLoading()
    {
        return isLoading;
    }

    public bool IsSceneLoaded(string sceneName)
    {
        return currentSceneName == sceneName;
    }

    public void Set_fadeColor(Color color)
    {
        _fadeColor = color;

        if (fadeObject != null)
        {
            UnityEngine.UI.Image fadeImage = fadeObject.GetComponentInChildren<UnityEngine.UI.Image>();
            if (fadeImage != null)
            {
                fadeImage.color = color;
            }
        }
    }

    #endregion
}