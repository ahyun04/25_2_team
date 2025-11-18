using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogManager : SingletonMono<DialogManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => false;

    [Header("데이터베이스")]
    [SerializeField] private DialogDatabaseSO _database;

    [Header("UI 컴포넌트")]
    public GameObject dialogPanel;
    public TextMeshProUGUI chatText;
    public TextMeshProUGUI nameText;

    [Header("선택지 UI")]
    public GameObject selectionUI;
    public Button button1;
    public TextMeshProUGUI button1Text;
    public Button button2;
    public TextMeshProUGUI button2Text;

    [Header("타이핑 속도")]
    [SerializeField] private float typingSpeed = 0.05f;

    [Tooltip("내부 상태 변수")]
    private DialogSO _currentDialog;
    private bool _isChoiceActive = false; // 선택지가 떠있는지
    private bool _isTyping = false;       // 텍스트가 타이핑 중인지
    private Coroutine _typingCoroutine;

    public System.Action OnDialogEnded;

    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (selectionUI != null) selectionUI.SetActive(false);
        if (_database != null) _database.Initialize();
    }

    #endregion

    #region 업데이트
    private void Update()
    {
        // UI가 꺼져있거나 선택지를 고르는 중이면 입력 무시
        if (dialogPanel.activeSelf == false || _isChoiceActive) return;

        // 엔터키 입력 처리
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // 텍스트가 타이핑 중이라면 -> 즉시 완성
            if (_isTyping)
            {
                CompleteTypingImmediately();
            }
            // 타이핑이 끝난 상태라면 -> 다음 대화로
            else
            {
                if (_currentDialog != null)
                {
                    if (_currentDialog.nextId == -1)
                    {
                        EndDialog();
                    }
                    else
                    {
                        ShowDialog(_currentDialog.nextId);
                    }
                }
            }
        }
    }

    #endregion

    #region 대화 시스템
    public void StartDialog(int dialogId)
    {
        if (_database == null) return;
        dialogPanel.SetActive(true);
        ShowDialog(dialogId);
    }

    public void EndDialog()
    {
        StopTyping();
        dialogPanel.SetActive(false);
        selectionUI.SetActive(false);
        _currentDialog = null;
        OnDialogEnded?.Invoke();
    }

    private void ShowDialog(int id)
    {
        _currentDialog = _database.GetDialogById(id);
        if (_currentDialog == null)
        {
            EndDialog();
            return;
        }

        nameText.text = _currentDialog.name;

        selectionUI.SetActive(false);
        StartTyping(_currentDialog.text);
    }

    #endregion

    #region 타이핑 로직
    private void StartTyping(string text)
    {
        StopTyping();
        _typingCoroutine = StartCoroutine(TypeTextRoutine(text));
    }

    private void StopTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        _isTyping = false;
    }

    private void CompleteTypingImmediately()
    {
        StopTyping();

        chatText.text = _currentDialog.text;
        _isTyping = false;

        CheckAndShowChoices();
    }

    private IEnumerator TypeTextRoutine(string text)
    {
        _isTyping = true;
        chatText.text = "";

        foreach (char c in text)
        {
            chatText.text += c;

            // 여기서 타이핑 사운드 넣으면 될듯?

            yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;

        CheckAndShowChoices();
    }

    #endregion

    #region 선택지 로직
    private void CheckAndShowChoices()
    {
        if (_currentDialog.choices != null && _currentDialog.choices.Count > 0)
        {
            ShowChoices(_currentDialog.choices);
        }
    }

    private void ShowChoices(List<DialogChoiceSO> choices)
    {
        _isChoiceActive = true;
        selectionUI.SetActive(true);

        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();

        if (choices.Count > 0)
        {
            button1.gameObject.SetActive(true);
            button1Text.text = choices[0].choiceText;
            int nextId = choices[0].choiceNextId;
            button1.onClick.AddListener(() => OnChoiceSelected(nextId));
        }
        else button1.gameObject.SetActive(false);

        if (choices.Count > 1)
        {
            button2.gameObject.SetActive(true);
            button2Text.text = choices[1].choiceText;
            int nextId = choices[1].choiceNextId;
            button2.onClick.AddListener(() => OnChoiceSelected(nextId));
        }
        else button2.gameObject.SetActive(false);
    }

    private void OnChoiceSelected(int nextId)
    {
        _isChoiceActive = false;
        ShowDialog(nextId);
    }

    #endregion
}