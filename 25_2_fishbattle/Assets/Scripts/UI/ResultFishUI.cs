using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultFishUI : MonoBehaviour
{
    [SerializeField] private UIModelViewer _modelViewer;
    [SerializeField] private TextMeshProUGUI _fishNameText;

    // FishSO 데이터를 받아 UI를 설정하는 함수
    public void SetData(FishSO fishData)
    {
        if (fishData == null) return;

        _modelViewer.ShowResultModel(fishData.Prefab);
        _fishNameText.text = fishData.Skill_name;
    }
}