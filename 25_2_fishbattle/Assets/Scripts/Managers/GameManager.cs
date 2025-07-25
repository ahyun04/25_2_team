using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver,
    Loading
}

public class GameManager : SingletonMono<GameManager>
{
    #region ���۷���
    protected override bool DontDestroy => true;

    [Header("���� ����")]
    [SerializeField] private GameState currentGameState = GameState.Menu;
    [SerializeField] private bool isGamePaused = false;

    [Header("���� ����")]
    [SerializeField] private float _gameTime = 0f;

    private GameState previousGameState;

    #endregion

    #region �ʱ�ȭ
    protected override void Awake()
    {
        base.Awake();
        InitializeGame();
    }

    // ���� �⺻ ����
    private void InitializeGame()
    {
        Application.targetFrameRate = 60;

        // �� ����� �߰� ����
    }

    private void Update()
    {
        if (currentGameState == GameState.Playing && !isGamePaused)
        {
            _gameTime += Time.deltaTime;
        }

        HandleInput();
    }

    #endregion

    #region (�׽�Ʈ)����
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentGameState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentGameState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }
    #endregion

    #region �� ��ȯ
    public void ChangeGameState(GameState newState)
    {
        if (currentGameState == newState) return;

        previousGameState = currentGameState;
        currentGameState = newState;

        OnGameStateChanged(newState);

        Debug.Log($"���� ���� ����: {previousGameState} -> {currentGameState}");
    }

    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Menu:

                break;
            case GameState.Playing:

                break;
            case GameState.Paused:

                break;
            case GameState.GameOver:

                break;
            case GameState.Loading:

                break;
        }
    }

    public void StartGame()
    {
        ChangeGameState(GameState.Playing);
        GameEvents.GameResumed();
    }

    public void PauseGame()
    {
        if (currentGameState != GameState.Playing) return;

        isGamePaused = true;
        Time.timeScale = 0f;
        ChangeGameState(GameState.Paused);
        GameEvents.GamePaused();
    }

    public void ResumeGame()
    {
        if (currentGameState != GameState.Paused) return;

        isGamePaused = false;
        Time.timeScale = 1f;
        ChangeGameState(GameState.Playing);
        GameEvents.GameResumed();
    }

    public void GameOver()
    {
        ChangeGameState(GameState.GameOver);
    }

    public void RestartGame()
    {
        ChangeGameState(GameState.Playing);
    }

    public void GoToMainMenu()
    {
        ChangeGameState(GameState.Menu);
    }

    #endregion

    #region ��ƿ��Ƽ
    public bool IsGamePlaying()
    {
        return currentGameState == GameState.Playing && !isGamePaused;
    }

    public void QuitGame()
    {
        Debug.Log("���� ����");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && currentGameState == GameState.Playing)
        {
            PauseGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && currentGameState == GameState.Playing)
        {
            PauseGame();
        }
    }

    #endregion
}