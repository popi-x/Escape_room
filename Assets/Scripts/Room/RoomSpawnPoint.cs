using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EscapeRoom.Rooms
{
    /// <summary>
    /// 标记玩家进入房间后的出生位置。
    /// 每个出生点都会自动生成一个隐藏标识，供场景切换时精确匹配。
    /// </summary>
    [AddComponentMenu("Escape Room/Rooms/Room Spawn Point")]
    [DisallowMultipleComponent]
    public sealed class RoomSpawnPoint : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField] private string spawnGuid = string.Empty;

        [Tooltip("可选显示名。Inspector 下拉里会优先显示这个名字。")]
        [SerializeField] private string displayName = string.Empty;

        /// <summary>
        /// 运行时使用的隐藏唯一标识。
        /// </summary>
        public string SpawnGuid => spawnGuid;

        /// <summary>
        /// Inspector 下拉里使用的显示文本。
        /// 没填显示名时，会退回物体名字。
        /// </summary>
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;

        private void Reset()
        {
            EnsureSpawnGuid();
        }

        private void OnValidate()
        {
            EnsureSpawnGuid();
        }

        /// <summary>
        /// 在 Scene 视图里画一个简单标记。
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.15f, 0.85f, 1f, 0.85f);
            Gizmos.DrawWireSphere(transform.position, 0.2f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.6f);
        }

        /// <summary>
        /// 自动补齐隐藏标识，避免依赖手填名字或字符串。
        /// </summary>
        private void EnsureSpawnGuid()
        {
            if (!string.IsNullOrWhiteSpace(spawnGuid))
            {
                return;
            }

#if UNITY_EDITOR
            spawnGuid = GUID.Generate().ToString();
            EditorUtility.SetDirty(this);
#endif
        }
    }
}
