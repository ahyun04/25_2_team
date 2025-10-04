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
    [SerializeField] private TextMeshProUGUI _netCooldownText;
    [SerializeField] private Button _netButton;

    [Header("제어할 스크립트")]
    [SerializeField] private NetFishing _netFishing;

    private bool _isCooldown = false;
    private float _cooldownTime = 15.0f;
    private float _cooldownTimer = 0f;

    #endregion

    #region 초기화
    void Start()
    {
        _netButton.onClick.AddListener(UseNetFish);
        _netCooldownText.gameObject.SetActive(false);
        _netCooldownImage.fillAmount = 0f;
    }

    #endregion

    #region 업데이트
    void Update()
    {
        if (_isCooldown)
        {
            ApplyCooldown();
        }
    }

    #endregion

    #region 쿨타임
    private void ApplyCooldown()
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
}