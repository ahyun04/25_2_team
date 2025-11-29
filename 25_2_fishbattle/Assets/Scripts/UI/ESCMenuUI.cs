using UnityEngine;

public class ESCMenuUI : MonoBehaviour
{
    public GameObject ESC_Panel;
    public Canvas _mainCanvas; // 렌더 모드를 바꿀 캔버스

    [Header("카메라 설정")]
    public Camera _mainCamera;       // 평소 게임 화면용 (MainCamera)
    public Camera _overlayCamera;    // 메뉴/UI 전용 카메라 (Overlay Camera)

    private bool isPaused = false;

    void Start()
    {
        if(_mainCamera == null) _mainCamera = Camera.main;
        if (_overlayCamera == null)
        {
            // "Overlay"라는 이름의 카메라 찾기 시도
            GameObject overlayCamObj = GameObject.Find("Overlay"); // 혹은 이름에 맞춰 수정
            if (overlayCamObj != null) _overlayCamera = overlayCamObj.GetComponent<Camera>();
        }
        if (ESC_Panel != null) ESC_Panel.SetActive(false);

        SetCanvasCamera(_mainCamera);

        // GameManager가 보내는 일시정지/재개 이벤트를 구독합니다.
        GameEvents.OnGamePaused += ShowPausePanel;
        GameEvents.OnGameResumed += HidePausePanel;
    }

    
    private void OnDestroy()
    {
        // 씬이 파괴될 때 이벤트 구독을 해제합니다.
        GameEvents.OnGamePaused -= ShowPausePanel;
        GameEvents.OnGameResumed -= HidePausePanel;
    }
    
    void Update()
    {
        // ESC 키로 직접 제어
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 게임 오버 상태일 때는 ESC 메뉴 작동 금지
            if (GameManager.Instance != null && GameManager.Instance.currentGameState == GameState.GameOver)
                return;

            if (isPaused) HidePausePanel();
            else ShowPausePanel();
        }
    }

    private void ShowPausePanel()
    {
        if (ESC_Panel != null) ESC_Panel.SetActive(true);

        SetCanvasCamera(_overlayCamera);

        Time.timeScale = 0f;
        isPaused = true;
    }

    private void HidePausePanel()
    {
        if (ESC_Panel != null) ESC_Panel.SetActive(false);

        SetCanvasCamera(_mainCamera);

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void SetCanvasCamera(Camera cam)
    {
        if (_mainCanvas != null && cam != null)
        {
            _mainCanvas.worldCamera = cam;
        }
    }
}
