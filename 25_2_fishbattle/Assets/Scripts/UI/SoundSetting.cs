using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSetting : MonoBehaviour
{
    public static SoundSetting Instance;

    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 슬라이더 값 불러오기 (없으면 기본값 1.0f)
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

        // 오디오 매니저에도 적용
        AudioManager.instance.MusicVolume(bgmSlider.value);
        AudioManager.instance.SFXVolume(sfxSlider.value);
    }

    public void MusicVolume()
    {
        AudioManager.instance.MusicVolume(bgmSlider.value);

        // 값 저장
        PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
    }

    public void SFXVolume()
    {
        AudioManager.instance.SFXVolume(sfxSlider.value);

        // 값 저장
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
    }
}
