using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MixerController : MonoBehaviour
{
    #region 레퍼런스
    [Header("Mixer")]
    [SerializeField] private AudioMixer _audioMixer;

    [Header("Slider")]
    [SerializeField] private Slider _musicMasterSlider;
    [SerializeField] private Slider _musicBGMSlider;
    [SerializeField] private Slider _musicSFXSlider;

    #endregion

    #region 초기화
    private void Awake()
    {
        _musicMasterSlider.value = PlayerPrefs.GetFloat("Volume_Master", 1f);
        _musicBGMSlider.value = PlayerPrefs.GetFloat("Volume_BGM", 1f);
        _musicSFXSlider.value = PlayerPrefs.GetFloat("Volume_SFX", 1f);

        _musicMasterSlider.onValueChanged.AddListener(SetMasterVolume);
        _musicBGMSlider.onValueChanged.AddListener(SetBGMVolume);
        _musicSFXSlider.onValueChanged.AddListener(SetSFXVolume);
    }
    #endregion

    #region 볼륨
    public void SetMasterVolume(float volume)                          
    {
        _audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);       
        PlayerPrefs.SetFloat("Volume_Master", volume);
    }

    public void SetBGMVolume(float volume)                       
    {
        _audioMixer.SetFloat("BGM", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("Volume_BGM", volume);
    }

    public void SetSFXVolume(float volume)                    
    {
        _audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("Volume_SFX", volume);
    }

    #endregion
}