using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoom.Progression
{
    /// <summary>
    /// 监听共享进度，并在结果变化时触发事件。
    /// </summary>
    [AddComponentMenu("Escape Room/Progress/Progress Condition Watcher")]
    [DisallowMultipleComponent]
    public sealed class ProgressConditionWatcher : MonoBehaviour
    {
        [Tooltip("这个监听器要检查的进度标记列表。")]
        [SerializeField] private List<ProgressFlagReference> requiredFlags = new List<ProgressFlagReference>();

        [Tooltip("开启时要求全部满足；关闭时只要任意一个满足。")]
        [SerializeField] private bool requireAllFlags = true;

        [Tooltip("把最终判断结果取反。")]
        [SerializeField] private bool invertResult = false;

        [Tooltip("组件启用时先按当前结果触发一次事件。")]
        [SerializeField] private bool invokeEventsOnEnable = true;

        [Header("Events")]
        [Tooltip("条件变成 true 时调用。")]
        [SerializeField] private UnityEvent onConditionMet;

        [Tooltip("条件变成 false 时调用。")]
        [SerializeField] private UnityEvent onConditionLost;

        private bool hasEvaluated;
        private bool currentResult;

        public bool CurrentResult => currentResult;

        private void OnEnable()
        {
            if (ProgressState.Instance != null)
            {
                ProgressState.Instance.FlagChanged += HandleFlagChanged;
            }

            Evaluate(invokeEventsOnEnable);
        }

        private void OnDisable()
        {
            if (ProgressState.Instance != null)
            {
                ProgressState.Instance.FlagChanged -= HandleFlagChanged;
            }
        }

        [ContextMenu("Evaluate Now")]
        public void EvaluateNow()
        {
            Evaluate(true);
        }

        private void HandleFlagChanged(string _, bool __)
        {
            Evaluate(false);
        }

        private void Evaluate(bool forceInvoke)
        {
            bool result = CalculateResult();

            if (!hasEvaluated)
            {
                hasEvaluated = true;
                currentResult = result;

                if (forceInvoke)
                {
                    InvokeCurrentResult();
                }

                return;
            }

            if (!forceInvoke && result == currentResult)
            {
                return;
            }

            currentResult = result;
            InvokeCurrentResult();
        }

        private bool CalculateResult()
        {
            if (requiredFlags.Count == 0)
            {
                return !invertResult;
            }

            if (ProgressState.Instance == null)
            {
                return false;
            }

            bool baseResult = requireAllFlags
                ? ProgressState.Instance.HasAllFlags(requiredFlags)
                : ProgressState.Instance.HasAnyFlag(requiredFlags);

            return invertResult ? !baseResult : baseResult;
        }

        private void InvokeCurrentResult()
        {
            if (currentResult)
            {
                onConditionMet?.Invoke();
            }
            else
            {
                onConditionLost?.Invoke();
            }
        }
    }
}
