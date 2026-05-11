using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EscapeRoom.Progression
{
    /// <summary>
    /// 标记一个可被全局进度系统读取的进度点。
    /// 使用者只需要在场景里放这个组件，并填写显示名。
    /// </summary>
    [AddComponentMenu("Escape Room/Progress/Progress Flag Definition")]
    [DisallowMultipleComponent]
    public sealed class ProgressFlagDefinition : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField] private string flagGuid = string.Empty;

        [Tooltip("Inspector 选择器里显示的名字。留空时会使用物体名。")]
        [SerializeField] private string displayName = string.Empty;

        public string FlagGuid => flagGuid;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;

        private void Reset()
        {
            EnsureFlagGuid();
        }

        private void OnValidate()
        {
            EnsureFlagGuid();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.7f, 0.15f, 0.85f);
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
        }

        private void EnsureFlagGuid()
        {
            if (!string.IsNullOrWhiteSpace(flagGuid))
            {
                return;
            }

#if UNITY_EDITOR
            flagGuid = GUID.Generate().ToString();
            EditorUtility.SetDirty(this);
#endif
        }
    }
}
