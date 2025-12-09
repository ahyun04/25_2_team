using UnityEngine;

public class UIModelViewer : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private Transform _modelContainer;
    [SerializeField] private LayerMask _uiLayer;

    private GameObject _currentModel;

    private void Awake()
    {
        if (_modelContainer == null) _modelContainer = transform;
    }

    // 모델 표시 함수
    public void ShowInventoryModel(GameObject prefab)
    {
        ClearModel();

        if (prefab == null) return;

        _currentModel = Instantiate(prefab, _modelContainer);

        // 트랜스폼 초기화 및 설정
        _currentModel.transform.localPosition = new Vector3(-44f, 0f, 0f);
        _currentModel.transform.localRotation = Quaternion.Euler(0f, -90f, 90f);;
        _currentModel.transform.localScale = new Vector3(20f, 20f, 20f);

        // 레이어 설정 (UI 카메라에 보이게)
        SetLayerRecursively(_currentModel, LayerMask.NameToLayer("UI"));
    }

    public void ShowCollectionModel(GameObject prefab)
    {
        ClearModel();

        if (prefab == null) return;

        _currentModel = Instantiate(prefab, _modelContainer);

        // 트랜스폼 초기화 및 설정
        _currentModel.transform.localPosition = new Vector3(-20f, 0f, 0f);
        _currentModel.transform.localRotation = Quaternion.Euler(0f, -90f, 90f); ;
        _currentModel.transform.localScale = new Vector3(10f, 10f, 10f);

        // 레이어 설정 (UI 카메라에 보이게)
        SetLayerRecursively(_currentModel, LayerMask.NameToLayer("UI"));
    }

    public void ShowResultModel(GameObject prefab)
    {
        ClearModel();

        if (prefab == null) return;

        _currentModel = Instantiate(prefab, _modelContainer);

        // 트랜스폼 초기화 및 설정
        _currentModel.transform.localPosition = new Vector3(-20f, 0f, 0f);
        _currentModel.transform.localRotation = Quaternion.Euler(0f, -90f, 90f); ;
        _currentModel.transform.localScale = new Vector3(10f, 10f, 10f);

        // 레이어 설정 (UI 카메라에 보이게)
        SetLayerRecursively(_currentModel, LayerMask.NameToLayer("UI"));
    }

    public void ShowEnchancementModel(GameObject prefab)
    {
        ClearModel();

        if (prefab == null) return;

        _currentModel = Instantiate(prefab, _modelContainer);

        // 트랜스폼 초기화 및 설정
        _currentModel.transform.localPosition = new Vector3(-20f, 0f, 0f);
        _currentModel.transform.localRotation = Quaternion.Euler(0f, -90f, 90f); ;
        _currentModel.transform.localScale = new Vector3(10f, 10f, 10f);

        // 레이어 설정 (UI 카메라에 보이게)
        SetLayerRecursively(_currentModel, LayerMask.NameToLayer("UI"));
    }

    // 모델 제거 함수
    public void ClearModel()
    {
        if (_currentModel != null)
        {
            Destroy(_currentModel);
            _currentModel = null;
        }
    }

    // 자식들까지 모두 레이어 변경 (UI 레이어로 맞춰야 UI 카메라에 보임)
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}