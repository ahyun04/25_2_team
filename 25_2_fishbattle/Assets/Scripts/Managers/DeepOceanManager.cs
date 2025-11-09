using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeepSeaManager : SingletonMono<DeepSeaManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => false;

    [Header("타이머 설정")]
    [Tooltip("심해에서 머무를 수 있는 시간 (초)")]
    [SerializeField] private float timeLimitInSeconds = 60f;

    [Header("UI 레퍼런스")]
    [SerializeField] private TextMeshProUGUI _timerText;

    private float _currentTimer;
    private bool _isTimerRunning = false;

    #endregion

    #region 초기화
    void Start()
    {
        _currentTimer = timeLimitInSeconds;
        _isTimerRunning = true;

        if (_timerText == null)
        {
            Debug.LogError("타이머 TextMeshProUGUI가 할당되지 않았습니다!");
        }
    }

    #endregion

    #region 업데이트
    void Update()
    {
        if (!_isTimerRunning) return;

        _currentTimer -= Time.deltaTime;

        UpdateTimerUI();

        if (_currentTimer <= 0)
        {
            TimeExpired();
        }
    }

    private void UpdateTimerUI()
    {
        if (_timerText != null)
        {
            _currentTimer = Mathf.Max(0, _currentTimer);
            float minutes = Mathf.FloorToInt(_currentTimer / 60);
            float seconds = Mathf.FloorToInt(_currentTimer % 60);
            _timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    #endregion

    #region 종료
    private void TimeExpired()
    {
        _isTimerRunning = false;
        Debug.Log("시간 종료! 바다 씬으로 돌아갑니다.");

        // 시간이 만료되면 바다 씬으로 강제 이동
        if (SceneManager.Instance != null)
            SceneManager.Instance.LoadScene(SceneManager.Instance.OceanGameSceneName);
    }

    #endregion
}