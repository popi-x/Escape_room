using System.Collections.Generic;
using EscapeRoom.Progression;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EscapeRoom.Rooms
{
    /// <summary>
    /// 门或传送入口，用来把玩家送到另一个房间 scene。
    /// 入口可以选择目标房间里的指定出生点，也可以回退到第一个出生点。
    /// </summary>
    [AddComponentMenu("Escape Room/Rooms/Room Portal")]
    [DisallowMultipleComponent]
    public sealed class RoomPortal : MonoBehaviour
    {
        public enum TriggerFilterMode
        {
            AnySource = 0,
            CurrentPlayerRig = 1,
            SpecificObjects = 2,
            LayerMask = 3,
        }

        [HideInInspector]
        [SerializeField] private string targetSceneName = string.Empty;

        [HideInInspector]
        [SerializeField] private string targetSpawnGuid = string.Empty;

#if UNITY_EDITOR
        [Header("Editor Only")]
        [Tooltip("目标房间 scene。")]
        [SerializeField] private SceneAsset targetScene;

        [HideInInspector]
        [SerializeField] private string targetSpawnDisplayName = string.Empty;
#endif

        [Header("Trigger")]
        [Tooltip("开启后，进入 Trigger 时会尝试切换房间。")]
        [SerializeField] private bool transitionOnTriggerEnter = false;

        [Tooltip("决定哪个物体可以触发这个入口。")]
        [SerializeField] private TriggerFilterMode triggerFilterMode = TriggerFilterMode.CurrentPlayerRig;

        [Tooltip("当触发模式为 Specific Objects 时，在这里拖允许触发的物体根节点。")]
        [SerializeField] private List<Transform> allowedTriggerRoots = new List<Transform>();

        [Tooltip("当触发模式为 Layer Mask 时，允许触发这个入口的 Layer。")]
        [SerializeField] private LayerMask allowedTriggerLayers = ~0;

        [Header("Lock State")]
        [Tooltip("开启后，入口在开始时是锁住的。")]
        [SerializeField] private bool startLocked = false;

        [Header("Requirements")]
        [Tooltip("使用这个入口前需要满足的进度标记。留空表示忽略全局进度。")]
        [SerializeField] private List<ProgressFlagReference> requiredFlags = new List<ProgressFlagReference>();

        [Tooltip("开启时要求全部 flag 满足；关闭时只要一个满足即可。")]
        [SerializeField] private bool requireAllFlags = true;

        [Header("Events")]
        [Tooltip("玩家尝试使用入口但被阻止时调用。")]
        [SerializeField] private UnityEvent onBlocked;

        [Tooltip("入口从锁住变成解锁时调用。")]
        [SerializeField] private UnityEvent onUnlocked;

        [Tooltip("入口从解锁变成锁住时调用。")]
        [SerializeField] private UnityEvent onLocked;

        [Tooltip("真正开始切换房间前调用。")]
        [SerializeField] private UnityEvent onTransitionRequested;

        private bool isLocked;
        private bool hasInitializedLockState;

        public string TargetSceneName => targetSceneName;
        public string TargetSpawnGuid => targetSpawnGuid;

#if UNITY_EDITOR
        public SceneAsset TargetSceneAsset => targetScene;

        public void SetEditorTargetSpawn(string spawnGuid, string displayName)
        {
            targetSpawnGuid = spawnGuid ?? string.Empty;
            targetSpawnDisplayName = displayName ?? string.Empty;
        }

        public string GetEditorTargetSpawnDisplayName()
        {
            return targetSpawnDisplayName;
        }
#endif

        /// <summary>
        /// 从编辑器里的 scene 引用同步出 scene 名称。
        /// </summary>
        private void OnValidate()
        {
#if UNITY_EDITOR
            targetSceneName = targetScene != null ? targetScene.name : string.Empty;

            if (targetScene == null)
            {
                targetSpawnGuid = string.Empty;
                targetSpawnDisplayName = string.Empty;
            }
#endif
        }

        /// <summary>
        /// 只初始化一次初始锁状态。
        /// </summary>
        private void Awake()
        {
            InitializeLockState();
        }

        private void OnEnable()
        {
            InitializeLockState();
        }

        /// <summary>
        /// 尝试使用这个入口。
        /// </summary>
        public void TryTransition()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            TryTransitionFromObject(null);
        }

        /// <summary>
        /// 尝试从指定对象发起入口切换。
        /// 可用于按钮、机关、检测器等非碰撞触发方式。
        /// </summary>
        public void TryTransitionFromObject(GameObject sourceObject)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (RoomFlowController.Instance == null)
            {
                Debug.LogWarning("RoomPortal: RoomFlowController was not found.");
                return;
            }

            if (string.IsNullOrWhiteSpace(targetSceneName))
            {
                Debug.LogWarning("RoomPortal: target scene is missing.");
                return;
            }

            if (!CanTransition())
            {
                onBlocked?.Invoke();
                return;
            }

            onTransitionRequested?.Invoke();
            if (sourceObject != null && !PassesSourceFilter(sourceObject.transform))
            {
                onBlocked?.Invoke();
                return;
            }

            RoomFlowController.Instance.RequestRoomChange(targetSceneName, targetSpawnGuid);
        }

        /// <summary>
        /// 可选的 Trigger 自动切换方式。
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (!transitionOnTriggerEnter)
            {
                return;
            }

            if (!PassesTriggerFilter(other))
            {
                return;
            }

            Transform sourceRoot = GetSourceRoot(other);
            TryTransitionFromObject(sourceRoot != null ? sourceRoot.gameObject : null);
        }

        /// <summary>
        /// 同时检查本地锁状态和全局进度条件。
        /// </summary>
        public bool CanTransition()
        {
            return !isLocked && CanPassProgressRequirements();
        }

        /// <summary>
        /// 解锁这个入口。
        /// </summary>
        public void UnlockPortal()
        {
            SetLocked(false);
        }

        /// <summary>
        /// 锁住这个入口。
        /// </summary>
        public void LockPortal()
        {
            SetLocked(true);
        }

        /// <summary>
        /// 设置本地锁状态。
        /// </summary>
        public void SetLocked(bool locked)
        {
            InitializeLockState();

            if (isLocked == locked)
            {
                return;
            }

            isLocked = locked;

            if (isLocked)
            {
                onLocked?.Invoke();
            }
            else
            {
                onUnlocked?.Invoke();
            }
        }

        /// <summary>
        /// 防止 Enable/Disable 后又把初始锁状态重置回去。
        /// </summary>
        private void InitializeLockState()
        {
            if (hasInitializedLockState)
            {
                return;
            }

            isLocked = startLocked;
            hasInitializedLockState = true;
        }

        /// <summary>
        /// 如果这个入口使用共享进度，就检查共享 flag。
        /// </summary>
        private bool CanPassProgressRequirements()
        {
            if (requiredFlags.Count == 0)
            {
                return true;
            }

            if (ProgressState.Instance == null)
            {
                return false;
            }

            return requireAllFlags
                ? ProgressState.Instance.HasAllFlags(requiredFlags)
                : ProgressState.Instance.HasAnyFlag(requiredFlags);
        }

        /// <summary>
        /// 按当前 Inspector 里选择的模式，判断这个碰撞体能不能触发入口。
        /// </summary>
        private bool PassesTriggerFilter(Collider other)
        {
            if (other == null)
            {
                return false;
            }

            Transform sourceRoot = GetSourceRoot(other);
            return PassesSourceFilter(sourceRoot);
        }

        /// <summary>
        /// 按当前模式判断这个来源能不能触发入口。
        /// </summary>
        private bool PassesSourceFilter(Transform sourceRoot)
        {
            switch (triggerFilterMode)
            {
                case TriggerFilterMode.AnySource:
                    return true;

                case TriggerFilterMode.CurrentPlayerRig:
                    return IsSameOrChildOf(sourceRoot, RoomFlowController.Instance != null ? RoomFlowController.Instance.PlayerRoot : null);

                case TriggerFilterMode.SpecificObjects:
                    return IsChildOfAnyAllowedRoot(sourceRoot);

                case TriggerFilterMode.LayerMask:
                    return sourceRoot != null && (allowedTriggerLayers.value & (1 << sourceRoot.gameObject.layer)) != 0;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 判断这个来源是否属于允许列表里的任意根物体。
        /// </summary>
        private bool IsChildOfAnyAllowedRoot(Transform sourceRoot)
        {
            for (int i = 0; i < allowedTriggerRoots.Count; i++)
            {
                if (IsSameOrChildOf(sourceRoot, allowedTriggerRoots[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 从碰撞体里提取触发来源的根节点。
        /// </summary>
        private static Transform GetSourceRoot(Collider other)
        {
            if (other == null)
            {
                return null;
            }

            return other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform;
        }

        /// <summary>
        /// 判断一个来源是否等于目标根物体，或属于它的子物体。
        /// </summary>
        private static bool IsSameOrChildOf(Transform sourceRoot, Transform root)
        {
            if (sourceRoot == null || root == null)
            {
                return false;
            }

            return sourceRoot == root || sourceRoot.IsChildOf(root);
        }
    }
}
