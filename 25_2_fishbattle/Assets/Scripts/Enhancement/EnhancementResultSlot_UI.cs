using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnhancementResultSlot_UI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private UIModelViewer _modelViewer;
    private EnhancementManager _manager;

    private bool _hasResult = false;

    private void Awake()
    {
        if (_modelViewer == null)
            _modelViewer = GetComponentInChildren<UIModelViewer>();
    }

    public void Initialize(EnhancementManager manager)
    {
        _manager = manager;
        Clear();
    }

    public void SetItem(FishSO fish)
    {
        if (fish != null)
        {
            _modelViewer.ShowEnchancementModel(fish.Prefab);
            _hasResult = true;
        }
    }

    public void Clear()
    {
        if (_modelViewer != null) _modelViewer.ClearModel();
        _hasResult = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && _hasResult)
        {
            _manager.OnResultSlotClick();
        }
    }
}