using UnityEngine;

public class ESCMenuUI : MonoBehaviour
{
    public GameObject ESC_Panel;
    private bool isPaused = false;


    void Start()
    {
        if (ESC_Panel != null)
        {
            ESC_Panel.SetActive(false);
        }

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
            if (isPaused)
            {
                HidePausePanel();
            }
            else
            {
                ShowPausePanel();
            }
        }
    }

    // 게임이 일시정지되면(알트탭 포함) 패널을 켭니다.
    private void ShowPausePanel()
    {
        if (ESC_Panel != null)
        {
            ESC_Panel.SetActive(true);
        }
        Time.timeScale = 0f;
        isPaused = true;
    }

    private void HidePausePanel()
    {
        if (ESC_Panel != null)
        {
            ESC_Panel.SetActive(false);
        }
        Time.timeScale = 1f;
        isPaused = false;
    }
}
