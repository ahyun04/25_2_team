using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnhancementResultSlot_UI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _resultImage; 
    private EnhancementManager _manager;

    public void Initialize(EnhancementManager manager)
    {
        _manager = manager;
        Clear();
    }

    public void SetItem(Sprite icon)
    {
        _resultImage.sprite = icon;
        _resultImage.gameObject.SetActive(true);
    }

    public void Clear()
    {
        _resultImage.sprite = null;
        _resultImage.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && _resultImage.gameObject.activeSelf)
        {
            _manager.OnResultSlotClick();
        }
    }
}