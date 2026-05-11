using EscapeRoom.Progression;
using EscapeRoom.Rooms;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EscapeRoom.Debugging
{
    /// <summary>
    /// 给开发阶段使用的简单调试入口。
    /// 可以跳房间、重置进度和打印当前状态。
    /// </summary>
    [AddComponentMenu("Escape Room/Debug/Room Debug Controller")]
    [DisallowMultipleComponent]
    public sealed class RoomDebugController : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField] private string selectedRoomSceneName = string.Empty;

        [HideInInspector]
        [SerializeField] private string selectedSpawnGuid = string.Empty;

#if UNITY_EDITOR
        [Header("Editor Only")]
        [Tooltip("调试跳房间时使用的目标场景。")]
        [SerializeField] private SceneAsset selectedRoomScene;

        [HideInInspector]
        [SerializeField] private string selectedSpawnDisplayName = string.Empty;
#endif

        [Tooltip("房间切换控制器。留空时会自动查找全局实例。")]
        [SerializeField] private RoomFlowController roomFlowController;

        [Tooltip("执行调试动作时在 Console 里输出日志。")]
        [SerializeField] private bool logActions = true;

#if UNITY_EDITOR
        public SceneAsset SelectedRoomSceneAsset => selectedRoomScene;
#endif

        private void OnValidate()
        {
            ResolveReferences();

#if UNITY_EDITOR
            selectedRoomSceneName = selectedRoomScene != null ? selectedRoomScene.name : string.Empty;
            if (selectedRoomScene == null)
            {
                selectedSpawnGuid = string.Empty;
                selectedSpawnDisplayName = string.Empty;
            }
#endif
        }

        [ContextMenu("Jump To Selected Room")]
        public void JumpToSelectedRoom()
        {
            ResolveReferences();

            if (roomFlowController == null)
            {
                Debug.LogWarning("RoomDebugController: RoomFlowController was not found.", this);
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedRoomSceneName))
            {
                Debug.LogWarning("RoomDebugController: selected room scene is missing.", this);
                return;
            }

            roomFlowController.RequestRoomChange(selectedRoomSceneName, selectedSpawnGuid);
            LogAction($"Jump to room '{selectedRoomSceneName}'.");
        }

        [ContextMenu("Reload Current Room")]
        public void ReloadCurrentRoom()
        {
            ResolveReferences();

            if (roomFlowController == null)
            {
                Debug.LogWarning("RoomDebugController: RoomFlowController was not found.", this);
                return;
            }

            roomFlowController.ReloadCurrentRoom();
            LogAction("Reload current room.");
        }

        [ContextMenu("Reset Progress")]
        public void ResetProgress()
        {
            if (ProgressState.Instance == null)
            {
                Debug.LogWarning("RoomDebugController: ProgressState was not found.", this);
                return;
            }

            ProgressState.Instance.ResetToInitialFlags();
            LogAction("Reset progress to initial flags.");
        }

        [ContextMenu("Reset Progress And Reload")]
        public void ResetProgressAndReload()
        {
            ResetProgress();
            ReloadCurrentRoom();
        }

        [ContextMenu("Print Current Room")]
        public void PrintCurrentRoom()
        {
            ResolveReferences();

            if (roomFlowController == null)
            {
                Debug.LogWarning("RoomDebugController: RoomFlowController was not found.", this);
                return;
            }

            string roomName = roomFlowController.CurrentRoom != null
                ? roomFlowController.CurrentRoom.DisplayName
                : roomFlowController.CurrentSceneName;

            Debug.Log($"RoomDebugController: current room = {roomName}", this);
        }

        [ContextMenu("Print Active Flags")]
        public void PrintActiveFlags()
        {
            if (ProgressState.Instance == null)
            {
                Debug.LogWarning("RoomDebugController: ProgressState was not found.", this);
                return;
            }

            ProgressState.Instance.PrintActiveFlags();
        }

        private void ResolveReferences()
        {
            if (roomFlowController == null)
            {
                roomFlowController = RoomFlowController.Instance != null
                    ? RoomFlowController.Instance
                    : FindFirstObjectByType<RoomFlowController>();
            }
        }

        private void LogAction(string message)
        {
            if (logActions)
            {
                Debug.Log($"RoomDebugController: {message}", this);
            }
        }
    }
}
