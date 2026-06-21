using Cinemachine;
using UnityEngine;

public enum CameraState
{
    Menu,
    LevelSelect,
    Combat,
    Shop,
    Result
}

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Virtual Cameras")]
    public CinemachineVirtualCamera menuCamera;
    public CinemachineVirtualCamera levelSelectCamera;
    public CinemachineVirtualCamera combatCamera;
    public CinemachineVirtualCamera shopCamera;
    public CinemachineVirtualCamera resultCamera;

    [Header("Priority")]
    public int activePriority = 20;
    public int inactivePriority = 0;

    public CameraState CurrentState { get; private set; } = CameraState.Combat;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetFollowTarget(Transform target)
    {
        if (combatCamera != null) combatCamera.Follow = target;
    }

    public void SwitchTo(CameraState state)
    {
        CurrentState = state;

        SetPriority(menuCamera, state == CameraState.Menu);
        SetPriority(levelSelectCamera, state == CameraState.LevelSelect);
        SetPriority(combatCamera, state == CameraState.Combat);
        SetPriority(shopCamera, state == CameraState.Shop);
        SetPriority(resultCamera, state == CameraState.Result);
    }

    public void SwitchToMenu() => SwitchTo(CameraState.Menu);
    public void SwitchToLevelSelect() => SwitchTo(CameraState.LevelSelect);
    public void SwitchToCombat()
    {
        EnsureCombatFollowTarget();
        SwitchTo(CameraState.Combat);
    }
    public void SwitchToShop() => SwitchTo(CameraState.Shop);
    public void SwitchToResult() => SwitchTo(CameraState.Result);

    private void SetPriority(CinemachineVirtualCamera targetCamera, bool isActive)
    {
        if (targetCamera == null) return;
        targetCamera.Priority = isActive ? activePriority : inactivePriority;
    }

    private void EnsureCombatFollowTarget()
    {
        if (combatCamera == null || combatCamera.Follow != null) return;

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            combatCamera.Follow = player.transform;
        }
    }
}
