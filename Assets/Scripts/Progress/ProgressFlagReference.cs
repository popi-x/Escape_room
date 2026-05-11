using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EscapeRoom.Progression
{
    /// <summary>
    /// 指向一个进度标记的隐藏引用。
    /// Inspector 里通过场景和下拉选择，不要求手填字符串。
    /// </summary>
    [Serializable]
    public sealed class ProgressFlagReference
    {
#if UNITY_EDITOR
        [Tooltip("进度标记所在的场景。")]
        [SerializeField] private SceneAsset scene;
#endif

        [HideInInspector]
        [SerializeField] private string sceneName = string.Empty;

        [HideInInspector]
        [SerializeField] private string flagGuid = string.Empty;

        [HideInInspector]
        [SerializeField] private string displayName = string.Empty;

        public string SceneName => sceneName;
        public string FlagGuid => flagGuid;
        public string DisplayName => displayName;
        public bool IsConfigured => !string.IsNullOrWhiteSpace(flagGuid);

#if UNITY_EDITOR
        public SceneAsset SceneAsset => scene;

        public void SetEditorSelection(string nextSceneName, string nextFlagGuid, string nextDisplayName)
        {
            sceneName = nextSceneName ?? string.Empty;
            flagGuid = nextFlagGuid ?? string.Empty;
            displayName = nextDisplayName ?? string.Empty;
        }

        public void SyncSceneName()
        {
            sceneName = scene != null ? scene.name : string.Empty;

            if (scene == null)
            {
                flagGuid = string.Empty;
                displayName = string.Empty;
            }
        }
#endif
    }
}
