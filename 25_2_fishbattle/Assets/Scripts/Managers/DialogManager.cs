using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
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

    [Header("사운드 설정")]
    public AudioSource voiceAudioSource;
    public AudioMixerGroup sfxMixerGroup;
    public AudioClip[] voiceClips;      
    [Tooltip("마침표(.), 쉼표(,), 말줄임표(...) 소리")]
    public AudioClip punctuationClip;

    [Range(0.5f, 2f)]
    public float minPitch = 0.9f;                               // 최소 음 높이
    [Range(0.5f, 2f)]
    public float maxPitch = 1.1f;                               // 최대 음 높이
    public bool stopAudioOnSpace = true;                        // 공백일 때 소리 안 낼지 여부

    private Queue<string> _sentences = new Queue<string>();     // 문장들을 담아둘 큐
    private string _currentSentence;                            // 현재 보여주고 있는 문장

    [Tooltip("내부 상태 변수")]
    private DialogSO _currentDialog;
    private bool _isChoiceActive = false;                       // 선택지가 떠있는지
    private bool _isTyping = false;                             // 텍스트가 타이핑 중인지
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

        if (voiceAudioSource == null)
            voiceAudioSource = GetComponent<AudioSource>();
        if (sfxMixerGroup != null)
            voiceAudioSource.outputAudioMixerGroup = sfxMixerGroup;
    }

    #endregion

    #region 업데이트
    private void Update()
    {
        // UI가 꺼져있거나 선택지를 고르는 중이면 입력 무시
        if (dialogPanel.activeSelf == false || _isChoiceActive) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_isTyping)
                CompleteTypingImmediately();
            else
            {
                if (_sentences.Count > 0)
                    DisplayNextSentence();
                else
                    MoveToNextDialog();
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

        _sentences.Clear();

        // '@' 문자를 기준으로 문장을 나눕니다. (엑셀에서 줄바꿈할 곳에 @를 넣으세요)
        string[] lines = _currentDialog.text.Split('@');

        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                _sentences.Enqueue(line.Trim());
            }
        }

        // 첫 번째 문장 출력 시작
        DisplayNextSentence();
    }
    private void DisplayNextSentence()
    {
        if (_sentences.Count == 0)
        {
            MoveToNextDialog();
            return;
        }

        _currentSentence = _sentences.Dequeue();
        StartTyping(_currentSentence);
    }

    private void MoveToNextDialog()
    {
        if (_currentDialog != null)
        {
            // 선택지가 있으면 선택지 표시 (텍스트 다 읽은 후)
            if (_currentDialog.choices != null && _currentDialog.choices.Count > 0)
            {
                CheckAndShowChoices();
                return;
            }

            // 선택지 없으면 nextId 체크
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

        chatText.text = _currentSentence;
        _isTyping = false;

        if (_sentences.Count == 0) CheckAndShowChoices();
    }

    private IEnumerator TypeTextRoutine(string text)
    {
        _isTyping = true;
        chatText.text = "";

        foreach (char c in text)
        {
            chatText.text += c;

            PlayTypingSound(c);

            if (c == '.' || c == ',' || c == '?' || c == '!')
                yield return new WaitForSeconds(typingSpeed * 2.5f);
            else
                yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;

        if (_sentences.Count == 0)
        {
            CheckAndShowChoices();
        }
    }

    private void PlayTypingSound(char c)
    {
        if (voiceAudioSource == null) return;

        if (stopAudioOnSpace && char.IsWhiteSpace(c)) return;
        if (c == '!' || c == '?') return;

        if (c == '.' || c == ',')
        {
            if (punctuationClip != null)
            {
                voiceAudioSource.clip = punctuationClip;
                voiceAudioSource.pitch = 1.0f;
                voiceAudioSource.Play();
                return;
            }
        }

        if (voiceClips.Length > 0)
        {
            int randomIndex = Random.Range(0, voiceClips.Length);
            voiceAudioSource.clip = voiceClips[randomIndex];

            int vowel = GetMiddleVowelIndex(c);
            float targetPitch = 1.0f;

            if (vowel != -1) 
            {
                // 모음 인덱스: 0:ㅏ, 4:ㅓ, 8:ㅗ, 13:ㅜ, 18:ㅡ, 20:ㅣ 등
                switch (vowel)
                {
                    case 0:  // ㅏ (밝음)
                    case 1:  // ㅐ
                    case 2:  // ㅑ
                    case 6:  // ㅕ
                        targetPitch = 1.2f; // 높게
                        break;

                    case 8:  // ㅗ (중간 높음)
                    case 12: // ㅛ
                    case 20: // ㅣ
                        targetPitch = 1.1f;
                        break;

                    case 4:  // ㅓ (중간)
                    case 5:  // ㅔ
                        targetPitch = 1.0f;
                        break;

                    case 13: // ㅜ (어두움)
                    case 17: // ㅠ
                    case 18: // ㅡ
                        targetPitch = 0.85f; // 낮게
                        break;

                    default: // 그 외 복합 모음들
                        targetPitch = 1.0f;
                        break;
                }

                // 약간의 랜덤성 추가 (너무 기계적이지 않게)
                targetPitch += Random.Range(-0.05f, 0.05f);
            }
            else // 한글이 아닐 경우 (영어, 숫자 등)
            {
                targetPitch = Random.Range(minPitch, maxPitch); // 기존 방식
            }

            voiceAudioSource.pitch = targetPitch;
            voiceAudioSource.Play();
        }
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
        
        // 예시로 -999 한것
        if (nextId == -999)
        {
            dialogPanel.SetActive(false);
            TradeManager.Instance.StartTrade();
            return;
        }

        ShowDialog(nextId);
    }

    #endregion

    #region 한글 유틸리티
    private int GetMiddleVowelIndex(char c)
    {
        if (c < 0xAC00 || c > 0xD7A3) return -1;

        int unicodeIndex = c - 0xAC00;
        int middleIndex = (unicodeIndex / 28) % 21;

        return middleIndex;
    }

    #endregion
}