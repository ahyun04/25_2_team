using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultFishUI : MonoBehaviour
{
    [SerializeField] private Image _fishImage;
    [SerializeField] private TextMeshProUGUI _fishNameText;

    // FishSO 데이터를 받아 UI를 설정하는 함수
    public void SetData(FishSO fishData)
    {
        if (fishData == null) return;
        _fishImage.sprite = fishData.Icon;
        _fishNameText.text = fishData.Name;
    }
}