using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FishingSystem
{
    // --- 그물 데이터 ---
    public bool IsNetOnCooldown { get; private set; }
    public float NetCooldownTimer { get; private set; }
    private const float NET_COOLDOWN_TIME = 15.0f;

    // --- 통발 데이터 ---
    public enum TrapState { Idle, Timing, ReadyToCollect }
    public TrapState CurrentTrapState { get; private set; } = TrapState.Idle;
    public bool IsTrapOnCooldown { get; private set; }
    public float TrapTimer { get; private set; }
    public float TrapCooldownTimer { get; private set; }
    private const float TRAP_DURATION = 30.0f;
    private const float TRAP_COOLDOWN_TIME = 20.0f;

    // --- 이벤트 ---
    public UnityAction OnFishingStateChanged;

    // 매 프레임 호출되어 타이머들을 업데이트합니다.
    public void UpdateTimers(float deltaTime)
    {
        bool changed = false;

        // 그물 쿨타임
        if (IsNetOnCooldown)
        {
            NetCooldownTimer -= deltaTime;
            if (NetCooldownTimer <= 0)
            {
                IsNetOnCooldown = false;
            }
            changed = true;
        }

        // 통발 타이머
        if (CurrentTrapState == TrapState.Timing)
        {
            TrapTimer -= deltaTime;
            if (TrapTimer <= 0)
            {
                CurrentTrapState = TrapState.ReadyToCollect;
                Debug.Log("통발 시간 끝! 수확 가능 상태입니다.");
            }
            changed = true;
        }

        // 통발 쿨타임
        if (IsTrapOnCooldown)
        {
            TrapCooldownTimer -= deltaTime;
            if (TrapCooldownTimer <= 0)
            {
                IsTrapOnCooldown = false;
            }
            changed = true;
        }

        // 변경 사항이 있을 때만 UI 업데이트 신호를 보냄
        if (changed)
        {
            OnFishingStateChanged?.Invoke();
        }
    }

    // --- 그물 관련 메서드 ---
    public bool CanUseNet() => !IsNetOnCooldown;
    public void StartNetCooldown()
    {
        if (IsNetOnCooldown) return;
        IsNetOnCooldown = true;
        NetCooldownTimer = NET_COOLDOWN_TIME;
        OnFishingStateChanged?.Invoke();
    }

    // --- 통발 관련 메서드 ---
    public bool CanStartTrap() => CurrentTrapState == TrapState.Idle && !IsTrapOnCooldown;
    public void StartTrap()
    {
        if (!CanStartTrap()) return;
        CurrentTrapState = TrapState.Timing;
        TrapTimer = TRAP_DURATION;
        StartTrapCooldown(); // 통발 설치와 동시에 버튼 쿨타임 시작
        OnFishingStateChanged?.Invoke();
    }

    public void CollectTrap()
    {
        if (CurrentTrapState != TrapState.ReadyToCollect) return;
        CurrentTrapState = TrapState.Idle; // 수확 후 다시 설치 가능 상태로
        OnFishingStateChanged?.Invoke();
    }

    private void StartTrapCooldown()
    {
        IsTrapOnCooldown = true;
        TrapCooldownTimer = TRAP_COOLDOWN_TIME;
    }
}