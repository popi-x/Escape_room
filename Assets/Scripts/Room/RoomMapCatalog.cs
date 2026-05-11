using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace EscapeRoom.Rooms
{
    /// <summary>
    /// 一条房间地图记录。
    /// 正常只需要拖场景，显示名和 minimap 数据会自动从 RoomSceneInfo 同步。
    /// </summary>
    [Serializable]
    public sealed class RoomMapCatalogEntry
    {
#if UNITY_EDITOR
        [Tooltip("房间 scene。")]
        [SerializeField] private SceneAsset scene;
#endif

        [HideInInspector]
        [SerializeField] private string sceneName = string.Empty;

        [HideInInspector]
        [SerializeField] private string displayName = string.Empty;

        [HideInInspector]
        [SerializeField] private Vector2Int minimapCoordinate = Vector2Int.zero;

        [HideInInspector]
        [SerializeField] private Color minimapColor = Color.white;

        public string SceneName => sceneName;
        public string DisplayName => displayName;
        public Vector2Int MinimapCoordinate => minimapCoordinate;
        public Color MinimapColor => minimapColor;
        public bool IsConfigured => !string.IsNullOrWhiteSpace(sceneName);

#if UNITY_EDITOR
        public SceneAsset SceneAsset => scene;

        public void SyncFromSceneInfo()
        {
            sceneName = scene != null ? scene.name : string.Empty;

            if (scene == null)
            {
                displayName = string.Empty;
                minimapCoordinate = Vector2Int.zero;
                minimapColor = Color.white;
                return;
            }

            string scenePath = AssetDatabase.GetAssetPath(scene);
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                return;
            }

            bool wasAlreadyOpen = TryGetOpenScene(scenePath, out Scene openScene);
            if (EditorApplication.isPlayingOrWillChangePlaymode && !wasAlreadyOpen)
            {
                return;
            }

            if (!wasAlreadyOpen)
            {
                openScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            try
            {
                RoomSceneInfo sceneInfo = FindRoomSceneInfo(openScene);
                if (sceneInfo != null)
                {
                    displayName = sceneInfo.DisplayName;
                    minimapCoordinate = sceneInfo.MinimapCoordinate;
                    minimapColor = sceneInfo.MinimapColor;
                }
                else
                {
                    displayName = scene.name;
                    minimapCoordinate = Vector2Int.zero;
                    minimapColor = Color.white;
                }
            }
            finally
            {
                if (!wasAlreadyOpen && openScene.IsValid())
                {
                    EditorSceneManager.CloseScene(openScene, true);
                }
            }
        }

        private static bool TryGetOpenScene(string scenePath, out Scene sceneResult)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.path == scenePath)
                {
                    sceneResult = scene;
                    return true;
                }
            }

            sceneResult = default;
            return false;
        }

        private static RoomSceneInfo FindRoomSceneInfo(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                RoomSceneInfo sceneInfo = roots[i].GetComponentInChildren<RoomSceneInfo>(true);
                if (sceneInfo != null)
                {
                    return sceneInfo;
                }
            }

            return null;
        }
#endif
    }

    /// <summary>
    /// 常驻场景里的房间地图总表。
    /// minimap 和调试工具从这里读取房间列表。
    /// </summary>
    [AddComponentMenu("Escape Room/Rooms/Room Map Catalog")]
    [DisallowMultipleComponent]
    public sealed class RoomMapCatalog : MonoBehaviour
    {
        [Tooltip("在这里拖所有正式房间 scene。房间信息会自动从 RoomSceneInfo 同步。")]
        [SerializeField] private List<RoomMapCatalogEntry> rooms = new List<RoomMapCatalogEntry>();

        public static RoomMapCatalog Instance { get; private set; }
        public IReadOnlyList<RoomMapCatalogEntry> Rooms => rooms;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("RoomMapCatalog: duplicate instance found.", this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            SyncAllEntriesFromScenes();
#endif
        }

#if UNITY_EDITOR
        [ContextMenu("Sync From Room Scene Info")]
        private void SyncAllEntriesFromScenes()
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i] != null)
                {
                    rooms[i].SyncFromSceneInfo();
                }
            }
        }
#endif

        public bool TryGetRoom(string sceneName, out RoomMapCatalogEntry entry)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                RoomMapCatalogEntry current = rooms[i];
                if (current == null || !current.IsConfigured)
                {
                    continue;
                }

                if (string.Equals(current.SceneName, sceneName, StringComparison.OrdinalIgnoreCase))
                {
                    entry = current;
                    return true;
                }
            }

            entry = null;
            return false;
        }

        public bool TryGetRoomAt(int index, out RoomMapCatalogEntry entry)
        {
            if (index >= 0 && index < rooms.Count)
            {
                entry = rooms[index];
                return entry != null && entry.IsConfigured;
            }

            entry = null;
            return false;
        }
    }
}
