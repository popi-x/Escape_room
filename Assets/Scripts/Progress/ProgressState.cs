using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoom.Progression
{
    /// <summary>
    /// 存放共享的 bool 进度状态。
    /// 内部用隐藏标识记录，Inspector 里通过进度标记选择。
    /// </summary>
    [AddComponentMenu("Escape Room/Progress/Progress State")]
    [DefaultExecutionOrder(-210)]
    [DisallowMultipleComponent]
    public sealed class ProgressState : MonoBehaviour
    {
        [Tooltip("游戏开始时默认为 true 的进度标记。")]
        [SerializeField] private List<ProgressFlagReference> initialTrueFlags = new List<ProgressFlagReference>();

        [Tooltip("每次进度变化时都输出 Console 日志。")]
        [SerializeField] private bool logChanges = false;

        private readonly HashSet<string> activeFlagGuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static ProgressState Instance { get; private set; }

        public event Action<string, bool> FlagChanged;
        public event Action<string, string, bool> FlagChangedDetailed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            RebuildInitialFlags();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool HasFlag(ProgressFlagReference flagReference)
        {
            return flagReference != null && HasFlagGuid(flagReference.FlagGuid);
        }

        public bool HasAllFlags(IEnumerable<ProgressFlagReference> flagReferences)
        {
            foreach (ProgressFlagReference flagReference in flagReferences)
            {
                if (flagReference == null || !flagReference.IsConfigured)
                {
                    continue;
                }

                if (!activeFlagGuids.Contains(flagReference.FlagGuid))
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasAnyFlag(IEnumerable<ProgressFlagReference> flagReferences)
        {
            foreach (ProgressFlagReference flagReference in flagReferences)
            {
                if (flagReference == null || !flagReference.IsConfigured)
                {
                    continue;
                }

                if (activeFlagGuids.Contains(flagReference.FlagGuid))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetFlag(ProgressFlagReference flagReference, bool value)
        {
            if (flagReference == null)
            {
                Debug.LogWarning("ProgressState: progress flag reference is missing.");
                return;
            }

            SetFlagGuid(flagReference.FlagGuid, flagReference.DisplayName, value);
        }

        public void SetTrue(ProgressFlagReference flagReference)
        {
            SetFlag(flagReference, true);
        }

        public void SetFalse(ProgressFlagReference flagReference)
        {
            SetFlag(flagReference, false);
        }

        public void Toggle(ProgressFlagReference flagReference)
        {
            if (flagReference == null)
            {
                Debug.LogWarning("ProgressState: progress flag reference is missing.");
                return;
            }

            SetFlagGuid(flagReference.FlagGuid, flagReference.DisplayName, !HasFlag(flagReference));
        }

        public string[] GetActiveFlagsSnapshot()
        {
            string[] results = new string[activeFlagGuids.Count];
            activeFlagGuids.CopyTo(results);
            Array.Sort(results, StringComparer.OrdinalIgnoreCase);
            return results;
        }

        [ContextMenu("Reset To Initial Flags")]
        public void ResetToInitialFlags()
        {
            activeFlagGuids.Clear();
            RebuildInitialFlags();
        }

        [ContextMenu("Print Active Flags")]
        public void PrintActiveFlags()
        {
            string[] flags = GetActiveFlagsSnapshot();
            string message = flags.Length == 0
                ? "(none)"
                : string.Join(", ", flags);

            Debug.Log($"ProgressState: active flags = {message}", this);
        }

        private bool HasFlagGuid(string flagGuid)
        {
            return !string.IsNullOrWhiteSpace(flagGuid) && activeFlagGuids.Contains(flagGuid);
        }

        private void SetFlagGuid(string flagGuid, string displayName, bool value)
        {
            if (string.IsNullOrWhiteSpace(flagGuid))
            {
                Debug.LogWarning("ProgressState: progress flag guid is missing.");
                return;
            }

            bool changed;
            if (value)
            {
                changed = activeFlagGuids.Add(flagGuid);
            }
            else
            {
                changed = activeFlagGuids.Remove(flagGuid);
            }

            if (!changed)
            {
                return;
            }

            if (logChanges)
            {
                string label = string.IsNullOrWhiteSpace(displayName) ? flagGuid : displayName;
                Debug.Log($"Progress flag changed: {label} = {value}");
            }

            FlagChanged?.Invoke(flagGuid, value);
            FlagChangedDetailed?.Invoke(flagGuid, displayName, value);
        }

        private void RebuildInitialFlags()
        {
            for (int i = 0; i < initialTrueFlags.Count; i++)
            {
                ProgressFlagReference flagReference = initialTrueFlags[i];
                if (flagReference == null || !flagReference.IsConfigured)
                {
                    continue;
                }

                activeFlagGuids.Add(flagReference.FlagGuid);
            }
        }
    }
}
