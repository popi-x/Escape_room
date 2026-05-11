using System;
using System.Collections;
using System.Collections.Generic;
using EscapeRoom.Progression;
using EscapeRoom.Rooms;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom.UI
{
    [AddComponentMenu("Escape Room/UI/Feedback UI Controller")]
    [DefaultExecutionOrder(-180)]
    [DisallowMultipleComponent]
    public sealed class FeedbackUIController : MonoBehaviour
    {
        public enum FeedbackKind
        {
            Info = 0,
            Success = 1,
            Warning = 2,
            Error = 3,
        }

        public enum FeedbackCanvasMode
        {
            WorldSpaceFollowHead = 0,
            ScreenSpaceOverlay = 1,
            Manual = 2,
        }

        [Serializable]
        private struct FeedbackStyle
        {
            public Color panelColor;
            public Color accentColor;
            public Color titleColor;
            public Color bodyColor;

            public FeedbackStyle(Color panelColor, Color accentColor, Color titleColor, Color bodyColor)
            {
                this.panelColor = panelColor;
                this.accentColor = accentColor;
                this.titleColor = titleColor;
                this.bodyColor = bodyColor;
            }
        }

        [Header("View")]
        [SerializeField] private FeedbackCanvasMode canvasMode = FeedbackCanvasMode.WorldSpaceFollowHead;
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform messageRoot;
        [SerializeField] private CanvasGroup messageCanvasGroup;
        [SerializeField] private Image messagePanel;
        [SerializeField] private Image accentBar;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Text legacyTitleText;
        [SerializeField] private Text legacyBodyText;
        [SerializeField] private RectTransform timerRoot;
        [SerializeField] private Image timerPanel;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private Text legacyTimerText;

        [Header("World Space")]
        [SerializeField] private Transform headOverride;
        [SerializeField] private Vector3 headSpaceOffset = new Vector3(0f, -0.08f, 0.6f);
        [SerializeField] private float worldCanvasScale = 0.0008f;
        [SerializeField] private float followLerp = 12f;

        [Header("Message Timing")]
        [SerializeField] private float defaultDuration = 2.5f;
        [SerializeField] private float instructionDuration = 4f;
        [SerializeField] private float fadeDuration = 0.18f;

        [Header("Auto Feedback")]
        [SerializeField] private bool announceProgressSolved = true;
        [SerializeField] private bool announceRoomChanges = false;
        [SerializeField] private string puzzleSolvedTitle = "Puzzle solved";
        [SerializeField] private string puzzleSolvedBodyFormat = "{0} solved.";
        [SerializeField] private string roomChangedTitle = "Current room";

        [Header("Default Messages")]
        [SerializeField] private string wrongPasswordTitle = "Wrong password";
        [SerializeField] private string wrongPasswordBody = "That code does not match.";
        [SerializeField] private string inputInstructionTitle = "Input required";
        [SerializeField] private string portalBlockedTitle = "Locked";
        [SerializeField] private string portalBlockedBody = "Something is still missing.";

        [Header("Styles")]
        [SerializeField]
        private FeedbackStyle infoStyle = new FeedbackStyle(
            new Color(0.04f, 0.055f, 0.075f, 0.92f),
            new Color(0.2f, 0.72f, 1f, 1f),
            Color.white,
            new Color(0.86f, 0.9f, 0.94f, 1f));

        [SerializeField]
        private FeedbackStyle successStyle = new FeedbackStyle(
            new Color(0.025f, 0.08f, 0.055f, 0.92f),
            new Color(0.25f, 0.86f, 0.45f, 1f),
            Color.white,
            new Color(0.86f, 0.95f, 0.88f, 1f));

        [SerializeField]
        private FeedbackStyle warningStyle = new FeedbackStyle(
            new Color(0.105f, 0.075f, 0.025f, 0.94f),
            new Color(1f, 0.72f, 0.18f, 1f),
            Color.white,
            new Color(1f, 0.92f, 0.76f, 1f));

        [SerializeField]
        private FeedbackStyle errorStyle = new FeedbackStyle(
            new Color(0.12f, 0.035f, 0.04f, 0.94f),
            new Color(1f, 0.24f, 0.24f, 1f),
            Color.white,
            new Color(1f, 0.84f, 0.84f, 1f));

        private readonly HashSet<string> announcedSolvedFlagGuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private Coroutine messageRoutine;
        private ProgressState subscribedProgressState;
        private RoomFlowController subscribedRoomFlow;

        public static FeedbackUIController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetMessageVisible(false, true);
        }

        private void OnEnable()
        {
            ResolveSubscriptions();
        }

        private void Update()
        {
            ResolveSubscriptions();
            UpdateFollowTransform();
        }

        private void OnDisable()
        {
            UnsubscribeProgress();
            UnsubscribeRoomFlow();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Show(FeedbackKind kind, string title, string body, float duration = -1f)
        {
            if (messageCanvasGroup == null)
            {
                return;
            }

            if (messageRoutine != null)
            {
                StopCoroutine(messageRoutine);
            }

            float resolvedDuration = duration < 0f ? defaultDuration : duration;
            messageRoutine = StartCoroutine(ShowMessageRoutine(kind, title, body, resolvedDuration));
        }

        public void ShowMessage(string message)
        {
            ShowInfo(message);
        }

        public void ShowInfo(string message)
        {
            Show(FeedbackKind.Info, string.Empty, message);
        }

        public void ShowSuccess(string message)
        {
            Show(FeedbackKind.Success, string.Empty, message);
        }

        public void ShowWarning(string message)
        {
            Show(FeedbackKind.Warning, string.Empty, message);
        }

        public void ShowError(string message)
        {
            Show(FeedbackKind.Error, string.Empty, message);
        }

        public void ShowPuzzleSolved()
        {
            ShowPuzzleSolved("Puzzle");
        }

        public void ShowPuzzleSolved(string puzzleName)
        {
            string label = string.IsNullOrWhiteSpace(puzzleName) ? "Puzzle" : puzzleName;
            Show(FeedbackKind.Success, puzzleSolvedTitle, string.Format(puzzleSolvedBodyFormat, label));
        }

        public void ShowWrongPassword()
        {
            Show(FeedbackKind.Error, wrongPasswordTitle, wrongPasswordBody);
        }

        public void ShowInputInstruction(string instruction)
        {
            string body = string.IsNullOrWhiteSpace(instruction) ? "Check the input and try again." : instruction;
            Show(FeedbackKind.Info, inputInstructionTitle, body, instructionDuration);
        }

        public void ShowPersistentInstruction(string instruction)
        {
            string body = string.IsNullOrWhiteSpace(instruction) ? "Check the input and try again." : instruction;
            Show(FeedbackKind.Info, inputInstructionTitle, body, 0f);
        }

        public void ShowPortalBlocked()
        {
            Show(FeedbackKind.Warning, portalBlockedTitle, portalBlockedBody);
        }

        public void Hide()
        {
            if (messageRoutine != null)
            {
                StopCoroutine(messageRoutine);
                messageRoutine = null;
            }

            SetMessageVisible(false, true);
        }

        public void SetTimerText(string text)
        {
            if (timerText == null && legacyTimerText == null)
            {
                return;
            }

            string safeText = text ?? string.Empty;

            if (timerText != null)
            {
                timerText.text = safeText;
            }

            if (legacyTimerText != null)
            {
                legacyTimerText.text = safeText;
            }

            GameObject root = timerRoot != null
                ? timerRoot.gameObject
                : timerText != null ? timerText.gameObject : legacyTimerText.gameObject;
            root.SetActive(!string.IsNullOrWhiteSpace(safeText));
        }

        public void ClearTimer()
        {
            SetTimerText(string.Empty);
        }

        public static void Notify(FeedbackKind kind, string title, string body, float duration = -1f)
        {
            if (Instance != null)
            {
                Instance.Show(kind, title, body, duration);
            }
        }

        private IEnumerator ShowMessageRoutine(FeedbackKind kind, string title, string body, float duration)
        {
            ApplyStyle(kind);
            ApplyText(title, body);
            SetMessageVisible(true, true);

            yield return FadeMessage(1f);

            if (duration > 0f)
            {
                yield return new WaitForSecondsRealtime(duration);
                yield return FadeMessage(0f);
                SetMessageVisible(false, true);
            }

            messageRoutine = null;
        }

        private IEnumerator FadeMessage(float targetAlpha)
        {
            if (messageCanvasGroup == null)
            {
                yield break;
            }

            if (fadeDuration <= 0f)
            {
                messageCanvasGroup.alpha = targetAlpha;
                yield break;
            }

            float startAlpha = messageCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                messageCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            messageCanvasGroup.alpha = targetAlpha;
        }

        private void ApplyStyle(FeedbackKind kind)
        {
            FeedbackStyle style = GetStyle(kind);

            if (messagePanel != null)
            {
                messagePanel.color = style.panelColor;
            }

            if (accentBar != null)
            {
                accentBar.color = style.accentColor;
            }

            if (titleText != null)
            {
                titleText.color = style.titleColor;
            }

            if (legacyTitleText != null)
            {
                legacyTitleText.color = style.titleColor;
            }

            if (bodyText != null)
            {
                bodyText.color = style.bodyColor;
            }

            if (legacyBodyText != null)
            {
                legacyBodyText.color = style.bodyColor;
            }
        }

        private void ApplyText(string title, string body)
        {
            bool hasTitle = !string.IsNullOrWhiteSpace(title);

            if (titleText != null)
            {
                titleText.text = title ?? string.Empty;
                titleText.gameObject.SetActive(hasTitle);
            }

            if (legacyTitleText != null)
            {
                legacyTitleText.text = title ?? string.Empty;
                legacyTitleText.gameObject.SetActive(hasTitle);
            }

            if (bodyText != null)
            {
                bodyText.text = body ?? string.Empty;
            }

            if (legacyBodyText != null)
            {
                legacyBodyText.text = body ?? string.Empty;
            }
        }

        private FeedbackStyle GetStyle(FeedbackKind kind)
        {
            switch (kind)
            {
                case FeedbackKind.Success:
                    return successStyle;
                case FeedbackKind.Warning:
                    return warningStyle;
                case FeedbackKind.Error:
                    return errorStyle;
                default:
                    return infoStyle;
            }
        }

        private void SetMessageVisible(bool visible, bool immediate)
        {
            if (messageRoot != null)
            {
                messageRoot.gameObject.SetActive(visible || !immediate);
            }

            if (messageCanvasGroup == null)
            {
                return;
            }

            if (immediate)
            {
                messageCanvasGroup.alpha = visible ? 1f : 0f;
            }

            messageCanvasGroup.interactable = false;
            messageCanvasGroup.blocksRaycasts = false;
        }

        private void ResolveSubscriptions()
        {
            ProgressState nextProgressState = ProgressState.Instance;
            if (subscribedProgressState != nextProgressState)
            {
                UnsubscribeProgress();
                subscribedProgressState = nextProgressState;

                if (subscribedProgressState != null)
                {
                    subscribedProgressState.FlagChangedDetailed += HandleFlagChangedDetailed;
                }
            }

            RoomFlowController nextRoomFlow = RoomFlowController.Instance;
            if (subscribedRoomFlow != nextRoomFlow)
            {
                UnsubscribeRoomFlow();
                subscribedRoomFlow = nextRoomFlow;

                if (subscribedRoomFlow != null)
                {
                    subscribedRoomFlow.CurrentRoomChanged += HandleCurrentRoomChanged;
                }
            }
        }

        private void UnsubscribeProgress()
        {
            if (subscribedProgressState == null)
            {
                return;
            }

            subscribedProgressState.FlagChangedDetailed -= HandleFlagChangedDetailed;
            subscribedProgressState = null;
        }

        private void UnsubscribeRoomFlow()
        {
            if (subscribedRoomFlow == null)
            {
                return;
            }

            subscribedRoomFlow.CurrentRoomChanged -= HandleCurrentRoomChanged;
            subscribedRoomFlow = null;
        }

        private void HandleFlagChangedDetailed(string flagGuid, string displayName, bool value)
        {
            if (!value)
            {
                announcedSolvedFlagGuids.Remove(flagGuid);
                return;
            }

            if (!announceProgressSolved || string.IsNullOrWhiteSpace(flagGuid))
            {
                return;
            }

            if (!announcedSolvedFlagGuids.Add(flagGuid))
            {
                return;
            }

            ShowPuzzleSolved(string.IsNullOrWhiteSpace(displayName) ? "Puzzle" : displayName);
        }

        private void HandleCurrentRoomChanged(RoomSceneInfo roomInfo)
        {
            if (!announceRoomChanges)
            {
                return;
            }

            string roomName = roomInfo != null && !string.IsNullOrWhiteSpace(roomInfo.DisplayName)
                ? roomInfo.DisplayName
                : RoomFlowController.Instance != null ? RoomFlowController.Instance.CurrentSceneName : string.Empty;

            if (!string.IsNullOrWhiteSpace(roomName))
            {
                Show(FeedbackKind.Info, roomChangedTitle, roomName);
            }
        }

        private void UpdateFollowTransform()
        {
            if (canvas == null || canvasMode != FeedbackCanvasMode.WorldSpaceFollowHead)
            {
                return;
            }

            Transform head = ResolveHeadTransform();
            if (head == null)
            {
                return;
            }

            if (canvas.worldCamera == null && Camera.main != null)
            {
                canvas.worldCamera = Camera.main;
            }

            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = head.forward;
            }

            forward.Normalize();
            Vector3 targetPosition = head.position +
                                     head.right * headSpaceOffset.x +
                                     Vector3.up * headSpaceOffset.y +
                                     forward * headSpaceOffset.z;
            Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);

            float t = followLerp <= 0f ? 1f : 1f - Mathf.Exp(-followLerp * Time.unscaledDeltaTime);
            Transform canvasTransform = canvas.transform;
            canvasTransform.position = Vector3.Lerp(canvasTransform.position, targetPosition, t);
            canvasTransform.rotation = Quaternion.Slerp(canvasTransform.rotation, targetRotation, t);
            canvasTransform.localScale = Vector3.one * worldCanvasScale;
        }

        private Transform ResolveHeadTransform()
        {
            if (headOverride != null)
            {
                return headOverride;
            }

            if (RoomFlowController.Instance != null && RoomFlowController.Instance.PlayerHead != null)
            {
                return RoomFlowController.Instance.PlayerHead;
            }

            return Camera.main != null ? Camera.main.transform : null;
        }
    }
}
