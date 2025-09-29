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

    [System.Serializable]
    public class PlayerEvents 
    {
        [Header("이동 Event")]
        public UnityEvent OnMoveStarted;
        public UnityEvent OnMoveStoped;
    }

    public PlayerEvents playerEvents;

    #endregion

    #region 업데이트
    private void Update()
    {
        HandleMovement();
    }

    #endregion

    #region 움직임
    private void HandleMovement()
    {
        // 입력 받기
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical);

        if (moveDirection.magnitude > 0.1f)
        {
            if (!_isMoving)
                StartMoving();

            // 이동 처리
            moveDirection = moveDirection.normalized;
            transform.Translate(moveDirection * _moveSpeed * Time.deltaTime, Space.World);

            // 회전 처리
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (_isMoving)
                StopMoving();
        }
    }

    #endregion

    #region 움직임 관련 Event 시스템
    private void StartMoving()
    {
        _isMoving = true;
        playerEvents.OnMoveStarted?.Invoke();
    }

    private void StopMoving()
    {
        _isMoving = false;
        playerEvents.OnMoveStoped?.Invoke();
    }

    #endregion
}
