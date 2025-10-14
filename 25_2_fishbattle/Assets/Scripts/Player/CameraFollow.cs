using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("따라갈 대상")]
    [SerializeField] private Transform _target;

    [Header("카메라 설정")]
    [SerializeField] private float _smoothSpeed = 0.125f;

    private Vector3 _offset;
    private Vector3 _velocity = Vector3.zero;

    void Start()
    {
        _offset = transform.position - _target.position;
    }

    void LateUpdate()
    {
        if (_target == null) return;

        Vector3 desiredPosition = _target.position + _offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _smoothSpeed);
    }
}