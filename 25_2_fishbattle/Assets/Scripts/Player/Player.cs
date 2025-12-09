using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SphereCollider))]
public class Player : MonoBehaviour
{
    #region 레퍼런스
    [Header("플레이어 설정")]
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _rotationSpeed = 10.0f;
    private bool _isMoving = false;
    public Animator _animator;
    /*public Animator _buketAnimator;
    public Animator _fishStickAnimator;*/

    private Rigidbody _rb;
    private Vector3 _moveInput;

    [Header("상호작용 & 카메라")]
    public CinemachineVirtualCamera mainCam;
    public CinemachineVirtualCamera dialogueCam;
    public CinemachineTargetGroup dialogueTargetGroup;
    public int activePriority = 11;
    public int inactivePriority = 9;
    public float dialogueRotationSpeed = 5.0f;

    private Coroutine _lookAtCoroutine;             // 바라보기 코루틴을 저장할 변수
    private NPC_AI_Controller _nearbyNPC;           // 상호작용 가능한 범위 내의 NPC
    private NPC_AI_Controller _activeDialogueNPC;   // 현재 대화 중인 NPC
    private bool _isInDialogue = false;
    public AudioClip Footstep;

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
            Debug.LogError("Player에 Rigidbody 컴포넌트가 없습니다");

        SphereCollider trigger = GetComponent<SphereCollider>();
        if (trigger != null)
            trigger.isTrigger = true;
        else
            Debug.LogError("Player에 SphereCollider가 없습니다");

        if (mainCam != null) mainCam.Priority = activePriority - 1;
        if (dialogueCam != null) dialogueCam.Priority = inactivePriority;
    }

    #endregion

    #region 업데이트
    private void Update()
    {
        // 현재 대화 중일 때
        if (_isInDialogue) return;

        // 대화 중이 아닐 때
        if (_nearbyNPC != null && Input.GetKeyDown(KeyCode.Space))
        {
            ToggleDialogue(_nearbyNPC);
            return;
        }

        HandleInputAndAnimation();

        

    }

    private void FixedUpdate()
    {
        if (_isInDialogue)
        {
            _rb.velocity = Vector3.zero;
            return;
        }

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
        AudioManager.instance.PlaySFX("Footstep");
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
        /*_buketAnimator.SetBool("Walk", true);
        _fishStickAnimator.SetBool("Walk", true);*/
        AudioManager.instance.PlaySFX("Footstep");
    }

    public void IdleAnimator()
    {
        _animator.SetBool("Walk", false);
        /*_buketAnimator.SetBool("Walk", false);
        _fishStickAnimator.SetBool("Walk", false);*/
    }

    #endregion

    #region 상호작용 (Trigger)
    private void OnTriggerEnter(Collider other)
    {
        if (_isInDialogue) return;

        if (other.TryGetComponent<NPC_AI_Controller>(out NPC_AI_Controller npc))
        {
            _nearbyNPC = npc;
            Debug.Log($"상호작용 가능: {npc.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<NPC_AI_Controller>(out NPC_AI_Controller npc))
        {
            if (_nearbyNPC == npc)
            {
                _nearbyNPC = null;
                Debug.Log($"상호작용 범위 이탈: {npc.name}");
            }
        }
    }

    #endregion

    #region 대화 제어
    private void ToggleDialogue(NPC_AI_Controller npc)
    {
        _isInDialogue = !_isInDialogue;

        if (_isInDialogue)
        {
            Debug.Log("대화 모드 시작");

            _activeDialogueNPC = npc;
            StopMoving();

            StartDialogueLook(_activeDialogueNPC.transform);

            _activeDialogueNPC.PauseMovement();
            _activeDialogueNPC.StartDialogueLook(this.transform);

            if (dialogueTargetGroup != null)
            {
                dialogueTargetGroup.m_Targets[1].target = npc.transform;
            }
            else
            {
                Debug.LogWarning("Dialogue Target Group이 Player 스크립트에 연결되지 않았습니다!");
            }

            if (dialogueCam != null) dialogueCam.Priority = activePriority;

            DialogManager.Instance.StartDialog(npc.startDialogId);

            // 대화가 끝났을 때 실행할 콜백 등록 (필요하다면)
            DialogManager.Instance.OnDialogEnded = () =>
            {
                // DialogManager가 대화를 끝내면 Player도 대화 모드 해제
                ToggleDialogue(_activeDialogueNPC); // 다시 호출하여 종료 로직 실행
            };
        }
        else
        {
            Debug.Log("대화 모드 종료");

            DialogManager.Instance.OnDialogEnded = null;

            if (_activeDialogueNPC == null) return;

            EndDialogueLook();

            _activeDialogueNPC.EndDialogueLook();
            _activeDialogueNPC.ResumeMovement();

            if (dialogueCam != null) dialogueCam.Priority = inactivePriority;

            _activeDialogueNPC = null;
        }
    }

    #endregion

    #region 대화 바라보기
    public void StartDialogueLook(Transform target)
    {
        if (_lookAtCoroutine != null)
        {
            StopCoroutine(_lookAtCoroutine);
        }

        _lookAtCoroutine = StartCoroutine(LookAtTargetRoutine(target));
    }

    public void EndDialogueLook()
    {
        if (_lookAtCoroutine != null)
        {
            StopCoroutine(_lookAtCoroutine);
            _lookAtCoroutine = null;
        }
    }

    private IEnumerator LookAtTargetRoutine(Transform target)
    {
        while (true)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    dialogueRotationSpeed * Time.deltaTime
                );
            }

            yield return null;
        }
    }

    #endregion
}
