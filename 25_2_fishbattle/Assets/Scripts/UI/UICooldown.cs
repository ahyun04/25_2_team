using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICooldown : MonoBehaviour
{
    #region 레퍼런스
    [Header("쿨타임 적용 UI")]
    [SerializeField] private Image _netCooldownImage;
    [SerializeField] private Image _trapCooldownImage;
    [SerializeField] private TextMeshProUGUI _netCooldownText;

    [Header("버튼")]
    [SerializeField] private Button _netButton;
    [SerializeField] private Button _fishTrapButton;

    [Header("제어할 스크립트")]
    [SerializeField] private NetFishing _netFishing;
    [SerializeField] private FishTrapManager _fishTrapManager;

    [Header("그물")]
    private bool _isCooldown = false;
    private float _cooldownTime = 15.0f;
    private float _cooldownTimer = 0f;

    [Header("통발")]
    private bool _isTrapCooldown = false;
    private float _trapCooldownTime = 20.0f;
    private float _trapCooldownTimer = 0f;

    #endregion

    #region 초기화
    void Start()
    {
        _netButton.onClick.AddListener(UseNetFish);
        _fishTrapButton.onClick.AddListener(UseFishTrap);

        _netCooldownText.gameObject.SetActive(false);
        _netCooldownImage.fillAmount = 0f;
        _trapCooldownImage.gameObject.SetActive(false);
    }

    #endregion

    #region 업데이트
    void Update()
    {
        if (_isCooldown)
        {
            ApplyNetCooldown();
        }
        if (_isTrapCooldown)
        {
            ApplyTrapCooldown();
        }
    }

    #endregion

    #region 그물 쿨타임
    private void ApplyNetCooldown()
    {
        _cooldownTimer -= Time.deltaTime;

        if (_cooldownTimer < 0.0f)
        {
            _isCooldown = false;
            _netCooldownText.gameObject.SetActive(false);
            _netCooldownImage.fillAmount = 0f;
            _netButton.interactable = true;
        }
        else
        {
            _netCooldownText.text = Mathf.RoundToInt(_cooldownTimer).ToString();
            _netCooldownImage.fillAmount = _cooldownTimer / _cooldownTime;
        }
    }

    // 버튼 클릭 시 호출: 쿨타임인지 '확인'하고 낚시 '시작 요청'만 함
    public void UseNetFish()
    {
        if (_isCooldown)
        {
            Debug.Log("아직 쿨타임입니다.");
            return;
        }
        else
        {
            // 쿨타임을 여기서 시작하지 않고, 낚시 시작만 요청
            _netFishing.StartNetFishing();
        }
    }

    // NetFishing 스크립트가 호출할 함수: 실제 쿨타임을 '시작'시킴
    public void StartCooldown()
    {
        _isCooldown = true;
        _netButton.interactable = false;
        _netCooldownText.gameObject.SetActive(true);
        _cooldownTimer = _cooldownTime;
    }

    #endregion

    #region 통발 쿨타임
    private void ApplyTrapCooldown()
    {
        _trapCooldownTimer -= Time.deltaTime;

        Debug.Log($"_trapCooldownTimer :{_trapCooldownTimer}");

        // 쿨타임이 끝나면
        if (_trapCooldownTimer < 0.0f)
        {
            _isTrapCooldown = false;
            _fishTrapButton.interactable = true;
            _trapCooldownImage.gameObject.SetActive(false);

            // 수확 대기 상태가 아니라면 쿨타임 이미지 숨기기
            if (_fishTrapManager.CurrentState == FishTrapManager.TrapState.Idle)
            {
                _trapCooldownImage.gameObject.SetActive(false);
            }
        }
    }

    // 통발 버튼 클릭 시 호출되는 메인 함수
    public void UseFishTrap()
    {
        // 통발이 설치 가능한 상태일 때만 작동합니다.
        if (_fishTrapManager.CurrentState == FishTrapManager.TrapState.Idle)
        {
            // 버튼 쿨타임 중인지 확인합니다.
            if (_isTrapCooldown)
            {
                Debug.Log("아직 쿨타임입니다.");
                return;
            }

            // 버튼을 비활성화하고 '사용 중' UI를 즉시 표시합니다.
            _fishTrapButton.interactable = false;
            _trapCooldownImage.gameObject.SetActive(true);

            // FishTrapManager에 30초 타이머 시작을 요청합니다.
            _fishTrapManager.StartTrap();
        }
    }

    // 버튼 쿨타임을 시작하는 별도 함수
    public void TriggerActualTrapCooldown()
    {
        _isTrapCooldown = true;
        _trapCooldownTimer = _trapCooldownTime;
    }

    #endregion
}