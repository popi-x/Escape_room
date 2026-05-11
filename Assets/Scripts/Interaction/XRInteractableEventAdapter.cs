using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace EscapeRoom.Interaction
{
    /// <summary>
    /// 把 XR Interactable 的事件转成普通 UnityEvent。
    /// 这样其他系统不需要直接依赖 XR 的事件参数类型。
    /// </summary>
    [AddComponentMenu("Escape Room/Interaction/XR Interactable Event Adapter")]
    [DisallowMultipleComponent]
    public sealed class XRInteractableEventAdapter : MonoBehaviour
    {
        [Tooltip("要监听的 XR Interactable。留空时会在当前物体上自动查找。")]
        [SerializeField] private XRBaseInteractable interactable;

        [Header("Events")]
        [Tooltip("第一次进入悬停时调用。")]
        [SerializeField] private UnityEvent onFirstHoverEntered;

        [Tooltip("进入悬停时调用。")]
        [SerializeField] private UnityEvent onHoverEntered;

        [Tooltip("离开悬停时调用。")]
        [SerializeField] private UnityEvent onHoverExited;

        [Tooltip("第一次选中时调用。")]
        [SerializeField] private UnityEvent onFirstSelectEntered;

        [Tooltip("选中时调用。")]
        [SerializeField] private UnityEvent onSelectEntered;

        [Tooltip("取消选中时调用。")]
        [SerializeField] private UnityEvent onSelectExited;

        [Tooltip("激活时调用。通常可以接按钮、机关和使用动作。")]
        [SerializeField] private UnityEvent onActivated;

        [Tooltip("取消激活时调用。")]
        [SerializeField] private UnityEvent onDeactivated;

        private XRBaseInteractable subscribedInteractable;

        private void Reset()
        {
            interactable = GetComponent<XRBaseInteractable>();
        }

        private void OnValidate()
        {
            if (interactable == null)
            {
                interactable = GetComponent<XRBaseInteractable>();
            }
        }

        private void OnEnable()
        {
            ResolveInteractable();
            RefreshSubscription();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// 如果 Interactable 引用变化，重新挂接事件。
        /// </summary>
        private void RefreshSubscription()
        {
            if (subscribedInteractable == interactable)
            {
                return;
            }

            Unsubscribe();

            if (interactable == null)
            {
                return;
            }

            subscribedInteractable = interactable;
            subscribedInteractable.firstHoverEntered.AddListener(HandleFirstHoverEntered);
            subscribedInteractable.hoverEntered.AddListener(HandleHoverEntered);
            subscribedInteractable.hoverExited.AddListener(HandleHoverExited);
            subscribedInteractable.firstSelectEntered.AddListener(HandleFirstSelectEntered);
            subscribedInteractable.selectEntered.AddListener(HandleSelectEntered);
            subscribedInteractable.selectExited.AddListener(HandleSelectExited);
            subscribedInteractable.activated.AddListener(HandleActivated);
            subscribedInteractable.deactivated.AddListener(HandleDeactivated);
        }

        /// <summary>
        /// 取消当前事件绑定，避免重复订阅。
        /// </summary>
        private void Unsubscribe()
        {
            if (subscribedInteractable == null)
            {
                return;
            }

            subscribedInteractable.firstHoverEntered.RemoveListener(HandleFirstHoverEntered);
            subscribedInteractable.hoverEntered.RemoveListener(HandleHoverEntered);
            subscribedInteractable.hoverExited.RemoveListener(HandleHoverExited);
            subscribedInteractable.firstSelectEntered.RemoveListener(HandleFirstSelectEntered);
            subscribedInteractable.selectEntered.RemoveListener(HandleSelectEntered);
            subscribedInteractable.selectExited.RemoveListener(HandleSelectExited);
            subscribedInteractable.activated.RemoveListener(HandleActivated);
            subscribedInteractable.deactivated.RemoveListener(HandleDeactivated);
            subscribedInteractable = null;
        }

        /// <summary>
        /// 自动补齐当前物体上的 XR Interactable。
        /// </summary>
        private void ResolveInteractable()
        {
            if (interactable == null)
            {
                interactable = GetComponent<XRBaseInteractable>();
            }
        }

        private void HandleFirstHoverEntered(HoverEnterEventArgs _)
        {
            onFirstHoverEntered?.Invoke();
        }

        private void HandleHoverEntered(HoverEnterEventArgs _)
        {
            onHoverEntered?.Invoke();
        }

        private void HandleHoverExited(HoverExitEventArgs _)
        {
            onHoverExited?.Invoke();
        }

        private void HandleFirstSelectEntered(SelectEnterEventArgs _)
        {
            onFirstSelectEntered?.Invoke();
        }

        private void HandleSelectEntered(SelectEnterEventArgs _)
        {
            onSelectEntered?.Invoke();
        }

        private void HandleSelectExited(SelectExitEventArgs _)
        {
            onSelectExited?.Invoke();
        }

        private void HandleActivated(ActivateEventArgs _)
        {
            onActivated?.Invoke();
        }

        private void HandleDeactivated(DeactivateEventArgs _)
        {
            onDeactivated?.Invoke();
        }
    }
}
