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

    [Header("Combat Follow")]
    public bool applyCombatLookAt = false;
    public Vector3 combatFollowOffset = new Vector3(0f, -0.75f, 0f);
    public bool enableFollowDebugLog = true;

    [Header("Runtime Debug")]
    [SerializeField] private string debugCurrentState;
    [SerializeField] private string debugTrackedTargetName;
    [SerializeField] private string debugCombatFollowName;
    [SerializeField] private string debugCombatLookAtName;
    [SerializeField] private string debugLiveVirtualCameraName;
    [SerializeField] private Vector3 debugMainCameraPosition;
    [SerializeField] private Vector3 debugCombatCameraPosition;
    [SerializeField] private Vector3 debugTrackedTargetPosition;

    public CameraState CurrentState { get; private set; } = CameraState.Combat;

    private Transform _currentFollowTarget;
    private string _lastLoggedFollowName;
    private CinemachineBrain _brain;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _brain = Camera.main != null ? Camera.main.GetComponent<CinemachineBrain>() : null;
    }

    private void LateUpdate()
    {
        UpdateDebugInfo();

        if (CurrentState != CameraState.Combat)
        {
            return;
        }

        if (_currentFollowTarget == null)
        {
            _currentFollowTarget = ResolvePlayerTarget();
            if (_currentFollowTarget != null)
            {
                ApplyCombatTarget();
            }
            return;
        }

        if (combatCamera != null && combatCamera.Follow == null)
        {
            ApplyCombatTarget();
        }

        LogFollowStateIfNeeded();
    }

    public void SetFollowTarget(Transform target)
    {
        _currentFollowTarget = target;
        ApplyCombatTarget();
        //LogDebug($"SetFollowTarget -> tracked={GetTransformPath(_currentFollowTarget)} follow={GetTransformPath(combatCamera != null ? combatCamera.Follow : null)}");
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
        EnsureCombatCameraPipeline();
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
        if (_currentFollowTarget != null)
        {
            ApplyCombatTarget();
            return;
        }

        if (combatCamera == null || combatCamera.Follow != null) return;

        Transform player = ResolvePlayerTarget();
        if (player != null)
        {
            _currentFollowTarget = player;
            ApplyCombatTarget();
            //LogDebug($"EnsureCombatFollowTarget -> resolved={GetTransformPath(player)}");
        }
    }

    private Transform ResolvePlayerTarget()
    {
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            PlayerController taggedController = taggedPlayer.GetComponent<PlayerController>();
            if (taggedController == null)
            {
                taggedController = taggedPlayer.GetComponentInParent<PlayerController>();
            }

            if (taggedController != null)
            {
                return taggedController.transform;
            }

            return taggedPlayer.transform;
        }

        PlayerController player = FindObjectOfType<PlayerController>();
        return player != null ? player.transform : null;
    }

    private void ApplyCombatTarget()
    {
        if (combatCamera == null)
        {
            return;
        }

        EnsureCombatCameraPipeline();

        if (_currentFollowTarget == null)
        {
            combatCamera.Follow = null;
            if (applyCombatLookAt)
            {
                combatCamera.LookAt = null;
            }
            return;
        }

        Transform followTarget = _currentFollowTarget;
        if (combatFollowOffset != Vector3.zero)
        {
            GameObject offsetAnchor = EnsureOffsetAnchor(_currentFollowTarget);
            if (offsetAnchor != null)
            {
                followTarget = offsetAnchor.transform;
            }
        }

        combatCamera.Follow = followTarget;
        if (applyCombatLookAt)
        {
            combatCamera.LookAt = followTarget;
        }

        //LogDebug($"ApplyCombatTarget -> tracked={GetTransformPath(_currentFollowTarget)} follow={GetTransformPath(followTarget)} lookAt={GetTransformPath(combatCamera.LookAt)}");
    }

    private void EnsureCombatCameraPipeline()
    {
        if (combatCamera == null)
        {
            return;
        }

        CinemachineFramingTransposer framing = combatCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framing == null)
        {
            framing = combatCamera.AddCinemachineComponent<CinemachineFramingTransposer>();
            //LogDebug("EnsureCombatCameraPipeline -> added CinemachineFramingTransposer");
        }

        framing.m_TrackedObjectOffset = Vector3.zero;
        framing.m_ScreenX = 0.5f;
        framing.m_ScreenY = 0.5f;
        framing.m_CameraDistance = 10f;
        framing.m_XDamping = 0.15f;
        framing.m_YDamping = 0.15f;
        framing.m_ZDamping = 0f;
        framing.m_DeadZoneWidth = 0f;
        framing.m_DeadZoneHeight = 0f;
        framing.m_SoftZoneWidth = 0.8f;
        framing.m_SoftZoneHeight = 0.8f;
        framing.m_BiasX = 0f;
        framing.m_BiasY = 0f;
        framing.m_UnlimitedSoftZone = false;
    }

    private GameObject EnsureOffsetAnchor(Transform target)
    {
        if (target == null)
        {
            return null;
        }

        const string anchorName = "CombatCameraFollowAnchor";
        Transform anchor = target.Find(anchorName);
        if (anchor == null)
        {
            GameObject anchorObject = new GameObject(anchorName);
            anchor = anchorObject.transform;
            anchor.SetParent(target, false);
        }

        anchor.localPosition = combatFollowOffset;
        anchor.localRotation = Quaternion.identity;
        anchor.localScale = Vector3.one;
        return anchor.gameObject;
    }

    private void UpdateDebugInfo()
    {
        if (_brain == null && Camera.main != null)
        {
            _brain = Camera.main.GetComponent<CinemachineBrain>();
        }

        debugCurrentState = CurrentState.ToString();
        debugTrackedTargetName = GetTransformPath(_currentFollowTarget);
        debugCombatFollowName = combatCamera != null ? GetTransformPath(combatCamera.Follow) : "<no combat camera>";
        debugCombatLookAtName = combatCamera != null ? GetTransformPath(combatCamera.LookAt) : "<no combat camera>";
        debugLiveVirtualCameraName = ResolveLiveVirtualCameraName();
        debugMainCameraPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        debugCombatCameraPosition = combatCamera != null ? combatCamera.transform.position : Vector3.zero;
        debugTrackedTargetPosition = _currentFollowTarget != null ? _currentFollowTarget.position : Vector3.zero;
    }

    private void LogFollowStateIfNeeded()
    {
        string followName = combatCamera != null ? GetTransformPath(combatCamera.Follow) : "<no combat camera>";
        if (followName == _lastLoggedFollowName)
        {
            return;
        }

        _lastLoggedFollowName = followName;
        //LogDebug($"FollowChanged -> state={CurrentState} tracked={GetTransformPath(_currentFollowTarget)} follow={followName}");
    }

    private void LogDebug(string message)
    {
        if (!enableFollowDebugLog)
        {
            return;
        }

        //Debug.Log($"[CameraDebug] {message}", this);
    }

    private static string GetTransformPath(Transform target)
    {
        if (target == null)
        {
            return "<null>";
        }

        string path = target.name;
        Transform current = target.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }

    private string ResolveLiveVirtualCameraName()
    {
        if (_brain == null)
        {
            return "<no brain>";
        }

        ICinemachineCamera liveCamera = _brain.ActiveVirtualCamera;
        if (liveCamera == null)
        {
            return "<none>";
        }

        return liveCamera.Name;
    }
}
