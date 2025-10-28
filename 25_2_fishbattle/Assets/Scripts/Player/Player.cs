using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    #region 레퍼런스
    [Header("플레이어 설정")]
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _rotationSpeed = 10.0f;
    private bool _isMoving = false;
    public Animator _animator;
    public Animator _buketAnimator;
    public Animator _fishStickAnimator;

    private Rigidbody _rb;
    private Vector3 _moveInput;

    [System.Serializable]
    public class PlayerEvents 
    {
        [Header("이동 Event")]
        public UnityEvent OnMoveStarted;
        public UnityEvent OnMoveStoped;
    }

    public PlayerEvents playerEvents;

    #endregion

    #region

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _rb = GetComponent<Rigidbody>();

        if (_rb == null) 
            Debug.LogError("Player에 Rigidbody 컴포넌트가 없습니다!");
    }

    #endregion

    #region 업데이트
    private void Update()
    {
        HandleInputAndAnimation();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    #endregion

    #region 움직임

    private void HandleInputAndAnimation()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        _moveInput = new Vector3(horizontal, 0, vertical).normalized;

        if (_moveInput.magnitude > 0.1f)
        {
            if (!_isMoving)
                StartMoving();
        }
        else
        {
            if (_isMoving)
                StopMoving();
        }
    }
    private void HandleMovement()
    {
        // 입력이 없으면 이동/회전 처리 안함
        if (_moveInput.magnitude < 0.1f) return;

        // 이동 처리
        // Space.World (월드 좌표 기준) 이동을 위해 현재 위치에서 더함
        Vector3 newPosition = _rb.position + _moveInput * _moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(newPosition);

        // 회전 처리
        if (_moveInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveInput);

            // 더 부드러운 물리 회전
            Quaternion newRotation = Quaternion.Slerp(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
            _rb.MoveRotation(newRotation);
        }
    }

    #endregion

    #region 움직임 관련 Event 시스템
    private void StartMoving()
    {
        _isMoving = true;
        WalkAnimator();
        playerEvents.OnMoveStarted?.Invoke();
    }

    private void StopMoving()
    {
        _isMoving = false;
        IdleAnimator();
        playerEvents.OnMoveStoped?.Invoke();
    }

    private void WalkAnimator()
    {
        _animator.SetBool("Walk", true);
        _buketAnimator.SetBool("Walk", true);
        _fishStickAnimator.SetBool("Walk", true);
    }

    public void IdleAnimator()
    {
        _animator.SetBool("Walk", false);
        _buketAnimator.SetBool("Walk", false);
        _fishStickAnimator.SetBool("Walk", false);
    }

    #endregion
}
