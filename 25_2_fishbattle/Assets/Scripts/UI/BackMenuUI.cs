using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackMenuUI : MonoBehaviour
{
    #region ���۷���
    [Header("���۷���")]
    [SerializeField] private Button _loadMainMenuButton;

    #endregion

    #region �ʱ�ȭ
    private void Start()
    {
        SetupButtons();

        // �̺�Ʈ ����
        GameEvents.OnSceneChanged += OnSceneChanged;
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        GameEvents.OnSceneChanged -= OnSceneChanged;
    }

    #endregion

    #region ��ư ����
    private void SetupButtons()
    {
        // �� �ε� ��ư
        if (_loadMainMenuButton != null)
            _loadMainMenuButton.onClick.AddListener(() => SceneManager.Instance.LoadMainMenu());
    }

    private void OnSceneChanged(string sceneName)
    {
        Debug.Log($"�� ���� �̺�Ʈ ����: {sceneName}");
    }

    #endregion
}
