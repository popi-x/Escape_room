using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoom.Progression
{
    /// <summary>
    /// 一个小工具，用来在 UnityEvent 里修改共享进度。
    /// 可以一次设置多个进度标记。
    /// </summary>
    [AddComponentMenu("Escape Room/Progress/Progress Flag Setter")]
    [DisallowMultipleComponent]
    public sealed class ProgressFlagSetter : MonoBehaviour
    {
        [Tooltip("SetConfiguredTrue、SetConfiguredFalse 和 ToggleConfigured 使用的进度标记列表。")]
        [SerializeField] private List<ProgressFlagReference> configuredFlags = new List<ProgressFlagReference>();

        public void SetConfiguredTrue()
        {
            ApplyToConfiguredFlags(true, false);
        }

        public void SetConfiguredFalse()
        {
            ApplyToConfiguredFlags(false, false);
        }

        public void ToggleConfigured()
        {
            ApplyToConfiguredFlags(false, true);
        }

        private void ApplyToConfiguredFlags(bool targetValue, bool toggle)
        {
            if (ProgressState.Instance == null)
            {
                return;
            }

            for (int i = 0; i < configuredFlags.Count; i++)
            {
                ProgressFlagReference flagReference = configuredFlags[i];
                if (flagReference == null || !flagReference.IsConfigured)
                {
                    continue;
                }

                if (toggle)
                {
                    ProgressState.Instance.Toggle(flagReference);
                }
                else
                {
                    ProgressState.Instance.SetFlag(flagReference, targetValue);
                }
            }
        }
    }
}
