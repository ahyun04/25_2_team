using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CollectionSlot_UI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _fishIconImage;

    private FishSO _assignedFish;
    private EncyclopediaUI _uiController;
    private bool _isCollected;

    private void Awake()
    {
        if (_fishIconImage == null)
            _fishIconImage = GetComponent<Image>();
    }

    public void SetupSlot(FishSO fish, bool isCollected, EncyclopediaUI controller)
    {
        _assignedFish = fish;
        _uiController = controller;
        _isCollected = isCollected;

        if (fish != null && _fishIconImage != null)
        {
            _fishIconImage.sprite = fish.Icon;

            if (isCollected)
            {
                _fishIconImage.color = Color.white; // 원본 색
            }
            else
            {
                _fishIconImage.color = Color.black; // 실루엣 처리 (또는 투명하게 하려면 a값 조절)
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isCollected)
        {
            _uiController.ShowDetailPanel(_assignedFish);
        }
    }
}