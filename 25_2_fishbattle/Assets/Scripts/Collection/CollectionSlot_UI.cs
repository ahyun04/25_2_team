using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CollectionSlot_UI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private UIModelViewer _modelViewer;

    private FishSO _assignedFish;
    private EncyclopediaUI _uiController;
    private bool _isCollected;

    private RectTransform _viewportRect;
    private RectTransform _myRect;
    private bool _isModelActive = false;

    private void Awake()
    {
        _myRect = GetComponent<RectTransform>();

        if (_modelViewer == null)
            _modelViewer = GetComponentInChildren<UIModelViewer>();
    }

    public void SetupSlot(FishSO fish, bool isCollected, EncyclopediaUI controller, RectTransform viewport)
    {
        _assignedFish = fish;
        _uiController = controller;
        _isCollected = isCollected;
        _viewportRect = viewport; // Viewport 저장

        if (fish != null && _modelViewer != null)
        {
            if (isCollected)
            {
                _modelViewer.ShowCollectionModel(fish.Prefab);
                _isModelActive = true;
            }
            else
            {
                _modelViewer.ClearModel();
                _isModelActive = false;
            }
        }
    }

    private void Update()
    {
        // 수집되지 않았거나, 뷰포트 정보가 없으면 계산 안 함
        if (!_isCollected || _viewportRect == null || !_isModelActive) return;
        if (_modelViewer == null) return;

        CheckVisibility();
    }

    private void CheckVisibility()
    {
        // 1. 내 위치(World)를 Viewport의 로컬 좌표로 변환
        Vector3 localPos = _viewportRect.InverseTransformPoint(_myRect.position);

        // 2. Viewport의 사각형(Rect) 안에 내 위치가 포함되는지 확인
        // (여유분을 주기 위해 y축 범위를 살짝 넓게 잡아도 좋습니다)
        if (_viewportRect.rect.Contains(localPos))
        {
            // 화면 안: 모델 보이기
            _modelViewer.gameObject.SetActive(true);
        }
        else
        {
            // 화면 밖: 모델 숨기기
            _modelViewer.gameObject.SetActive(false);
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