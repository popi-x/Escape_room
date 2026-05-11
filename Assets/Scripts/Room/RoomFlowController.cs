using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EscapeRoom.Rooms
{
    /// <summary>
    /// 全局房间切换控制器。
    /// 放在常驻场景里，只负责房间加载、卸载和玩家落点。
    /// </summary>
    [AddComponentMenu("Escape Room/Rooms/Room Flow Controller")]
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class RoomFlowController : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField] private string initialSceneName = string.Empty;

        [HideInInspector]
        [SerializeField] private string initialSpawnGuid = string.Empty;

#if UNITY_EDITOR
        [Header("Editor Only")]
        [Tooltip("游戏开始时先加载的房间 scene。")]
        [SerializeField] private SceneAsset initialScene;

        [HideInInspector]
        [SerializeField] private string initialSpawnDisplayName = string.Empty;
#endif

        [Header("Player")]
        [Tooltip("XR Rig 根物体。切换房间时会移动这个对象。")]
        [SerializeField] private Transform playerRoot;

        [Tooltip("玩家头部相机，用来读取朝向。")]
        [SerializeField] private Transform playerHead;

        [Header("Options")]
        [Tooltip("加载新房间后卸载上一个房间。")]
        [SerializeField] private bool unloadPreviousRoom = true;

        [Tooltip("把新加载的房间 scene 设为 Active Scene。")]
        [SerializeField] private bool setLoadedRoomAsActiveScene = false;

        [Tooltip("进入过的房间会被记录下来，给 minimap 和调试工具使用。")]
        [SerializeField] private bool trackVisitedRooms = true;

        private readonly HashSet<string> visitedSceneNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private Coroutine transitionRoutine;
        private string loadedSceneName = string.Empty;
        private string loadedSpawnGuid = string.Empty;

        public static RoomFlowController Instance { get; private set; }

        public event Action<RoomSceneInfo> CurrentRoomChanged;

        public RoomSceneInfo CurrentRoom { get; private set; }
        public Transform PlayerRoot => playerRoot;
        public Transform PlayerHead => playerHead;
        public string CurrentSceneName => loadedSceneName;
        public bool IsTransitioning => transitionRoutine != null;

#if UNITY_EDITOR
        public SceneAsset InitialSceneAsset => initialScene;

        public void SetEditorInitialSpawn(string spawnGuid, string displayName)
        {
            initialSpawnGuid = spawnGuid ?? string.Empty;
            initialSpawnDisplayName = displayName ?? string.Empty;
        }

        public string GetEditorInitialSpawnDisplayName()
        {
            return initialSpawnDisplayName;
        }
#endif

        /// <summary>
        /// 保持一个全局实例，并在房间切换时不销毁。
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            ResolveReferences();
        }

        /// <summary>
        /// 游戏开始时加载第一个房间。
        /// </summary>
        private void Start()
        {
            if (!string.IsNullOrWhiteSpace(initialSceneName))
            {
                RequestRoomChange(initialSceneName, initialSpawnGuid);
            }
        }

        /// <summary>
        /// 从编辑器里的 scene 引用同步出 scene 名称。
        /// </summary>
        private void OnValidate()
        {
#if UNITY_EDITOR
            initialSceneName = initialScene != null ? initialScene.name : string.Empty;

            if (initialScene == null)
            {
                initialSpawnGuid = string.Empty;
                initialSpawnDisplayName = string.Empty;
            }
#endif
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 给 UnityEvent 用的简化入口。
        /// </summary>
        public void RequestRoomChange(string targetSceneName)
        {
            RequestRoomChange(targetSceneName, string.Empty);
        }

        /// <summary>
        /// 切换到目标房间，并尽量落到指定出生点。
        /// </summary>
        public void RequestRoomChange(string targetSceneName, string targetSpawnGuid)
        {
            RequestRoomChangeInternal(targetSceneName, targetSpawnGuid, false);
        }

        /// <summary>
        /// 重新加载当前房间。
        /// 调试和重置流程可以用这个入口。
        /// </summary>
        public void ReloadCurrentRoom()
        {
            if (string.IsNullOrWhiteSpace(loadedSceneName))
            {
                Debug.LogWarning("RoomFlowController: current room is missing.");
                return;
            }

            RequestRoomChangeInternal(loadedSceneName, loadedSpawnGuid, true);
        }

        /// <summary>
        /// 检查一个房间是否访问过。
        /// minimap 和提示系统可以用这里判断。
        /// </summary>
        public bool HasVisitedRoom(string sceneName)
        {
            return !string.IsNullOrWhiteSpace(sceneName) && visitedSceneNames.Contains(sceneName);
        }

        /// <summary>
        /// 房间切换的统一入口。
        /// 只有这里负责启动真正的切换流程。
        /// </summary>
        private void RequestRoomChangeInternal(string targetSceneName, string targetSpawnGuid, bool forceReload)
        {
            if (string.IsNullOrWhiteSpace(targetSceneName))
            {
                Debug.LogWarning("RoomFlowController: target scene name is missing.");
                return;
            }

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            transitionRoutine = StartCoroutine(TransitionRoutine(targetSceneName, targetSpawnGuid, forceReload));
        }

        /// <summary>
        /// 加载目标房间，移动玩家，并更新当前房间信息。
        /// </summary>
        private IEnumerator TransitionRoutine(string targetSceneName, string targetSpawnGuid, bool forceReload)
        {
            ResolveReferences();

            if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                Debug.LogWarning($"RoomFlowController: scene '{targetSceneName}' is not in Build Settings.");
                transitionRoutine = null;
                yield break;
            }

            bool targetAlreadyLoaded = string.Equals(loadedSceneName, targetSceneName, StringComparison.OrdinalIgnoreCase);
            if (targetAlreadyLoaded && !forceReload)
            {
                Scene sameScene = SceneManager.GetSceneByName(targetSceneName);
                if (sameScene.IsValid() && sameScene.isLoaded)
                {
                    MovePlayerToSpawn(sameScene, targetSpawnGuid);
                    UpdateCurrentRoomInfo(sameScene);
                    loadedSpawnGuid = targetSpawnGuid;
                }

                transitionRoutine = null;
                yield break;
            }

            string sceneToUnload = string.Empty;
            if (targetAlreadyLoaded && forceReload)
            {
                sceneToUnload = targetSceneName;
            }
            else if (!string.IsNullOrWhiteSpace(loadedSceneName) && unloadPreviousRoom)
            {
                sceneToUnload = loadedSceneName;
            }

            if (!string.IsNullOrWhiteSpace(sceneToUnload))
            {
                Scene previousScene = SceneManager.GetSceneByName(sceneToUnload);
                if (previousScene.IsValid() && previousScene.isLoaded)
                {
                    AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(previousScene);
                    while (unloadOperation != null && !unloadOperation.isDone)
                    {
                        yield return null;
                    }
                }
            }

            Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
            if (!targetScene.isLoaded)
            {
                AsyncOperation loadOperation = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
                while (loadOperation != null && !loadOperation.isDone)
                {
                    yield return null;
                }

                targetScene = SceneManager.GetSceneByName(targetSceneName);
            }

            if (setLoadedRoomAsActiveScene && targetScene.IsValid())
            {
                SceneManager.SetActiveScene(targetScene);
            }

            loadedSceneName = targetSceneName;
            loadedSpawnGuid = targetSpawnGuid ?? string.Empty;

            // 等一帧，让房间里的对象先完成初始化。
            yield return null;

            MovePlayerToSpawn(targetScene, targetSpawnGuid);
            UpdateCurrentRoomInfo(targetScene);
            transitionRoutine = null;
        }

        /// <summary>
        /// 如果可能，自动补齐玩家引用。
        /// </summary>
        private void ResolveReferences()
        {
            if (playerHead == null && Camera.main != null)
            {
                playerHead = Camera.main.transform;
            }

            if (playerRoot == null && playerHead != null)
            {
                playerRoot = playerHead.root;
            }
        }

        /// <summary>
        /// 在目标房间里找到匹配的出生点，并把玩家放过去。
        /// 没指定目标时，会回退到该房间里的第一个出生点。
        /// </summary>
        private void MovePlayerToSpawn(Scene scene, string preferredSpawnGuid)
        {
            if (!scene.IsValid() || !scene.isLoaded || playerRoot == null)
            {
                return;
            }

            RoomSpawnPoint[] spawnPoints = FindObjectsByType<RoomSpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            RoomSpawnPoint firstSpawnInScene = null;
            RoomSpawnPoint matchedSpawn = null;

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                RoomSpawnPoint spawnPoint = spawnPoints[i];
                if (spawnPoint.gameObject.scene != scene)
                {
                    continue;
                }

                if (firstSpawnInScene == null)
                {
                    firstSpawnInScene = spawnPoint;
                }

                if (!string.IsNullOrWhiteSpace(preferredSpawnGuid) &&
                    string.Equals(spawnPoint.SpawnGuid, preferredSpawnGuid, StringComparison.Ordinal))
                {
                    matchedSpawn = spawnPoint;
                    break;
                }
            }

            RoomSpawnPoint finalSpawn = matchedSpawn != null ? matchedSpawn : firstSpawnInScene;
            if (finalSpawn != null)
            {
                AlignPlayerToSpawn(finalSpawn.transform);
            }
            else
            {
                Debug.LogWarning($"RoomFlowController: no RoomSpawnPoint was found inside scene '{scene.name}'.");
            }
        }

        /// <summary>
        /// 刷新当前房间信息，并把它记成已访问。
        /// </summary>
        private void UpdateCurrentRoomInfo(Scene scene)
        {
            CurrentRoom = FindRoomSceneInfo(scene);

            if (trackVisitedRooms && !string.IsNullOrWhiteSpace(scene.name))
            {
                visitedSceneNames.Add(scene.name);
            }

            CurrentRoomChanged?.Invoke(CurrentRoom);
        }

        /// <summary>
        /// 从已加载房间里读取房间信息组件。
        /// </summary>
        private RoomSceneInfo FindRoomSceneInfo(Scene scene)
        {
            RoomSceneInfo[] roomInfos = FindObjectsByType<RoomSceneInfo>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < roomInfos.Length; i++)
            {
                if (roomInfos[i].gameObject.scene == scene)
                {
                    return roomInfos[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 把玩家 Rig 移到出生点，并对齐朝向。
        /// </summary>
        private void AlignPlayerToSpawn(Transform spawnPoint)
        {
            Transform referenceHead = playerHead != null ? playerHead : playerRoot;

            Vector3 currentForward = Vector3.ProjectOnPlane(referenceHead.forward, Vector3.up);
            Vector3 targetForward = Vector3.ProjectOnPlane(spawnPoint.forward, Vector3.up);
            if (currentForward.sqrMagnitude > 0.0001f && targetForward.sqrMagnitude > 0.0001f)
            {
                float deltaAngle = Vector3.SignedAngle(currentForward, targetForward, Vector3.up);
                playerRoot.Rotate(Vector3.up, deltaAngle, Space.World);
            }

            Vector3 headOffset = playerRoot.position - referenceHead.position;
            headOffset.y = 0f;
            playerRoot.position = spawnPoint.position + headOffset;
        }
    }
}
