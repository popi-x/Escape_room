using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoom.UI
{
    [AddComponentMenu("Escape Room/UI/Look Hint Prompt")]
    public sealed class LookHintPrompt : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FeedbackUIController feedbackUI;
        [SerializeField] private Transform head;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform lookTarget;
        [SerializeField] private Collider rangeCollider;

        [Header("Look Check")]
        [SerializeField] private bool ignoreRangeHeight = true;
        [SerializeField] private float maxLookAngle = 25f;
        [SerializeField] private float lookHoldSeconds = 1f;

        [Header("Message")]
        [SerializeField] private FeedbackUIController.FeedbackKind kind = FeedbackUIController.FeedbackKind.Info;
        [SerializeField] private string title = "Hint";
        [SerializeField, TextArea] private string message = "Look closely to find how this puzzle works.";
        [SerializeField] private float duration = 3f;
        [SerializeField] private float repeatDelay = 8f;
        [SerializeField] private bool showOnlyOnce;

        [Header("Events")]
        [SerializeField] private UnityEvent shown;

        [Header("Debug")]
        [SerializeField] private bool debugLog;

        private bool hasShown;
        private float nextShowTime;
        private float lookTimer;
        private float nextDebugLogTime;
        private bool persistentMessageVisible;
        private Vector3 lastRangeCheckPosition;
        private Vector3 lastRangeClosestPoint;
        private float lastRangeSqrDistance;

        private void Reset()
        {
            lookTarget = transform;
            rangeCollider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (showOnlyOnce && hasShown)
            {
                return;
            }

            Transform resolvedHead = head != null ? head : Camera.main != null ? Camera.main.transform : null;
            Transform resolvedTarget = lookTarget != null ? lookTarget : transform;
            if (resolvedHead == null || resolvedTarget == null)
            {
                HidePersistentMessage();
                LogState("missing head/target", resolvedHead, resolvedTarget, false, 0f);
                return;
            }

            Vector3 toTarget = resolvedTarget.position - resolvedHead.position;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                lookTimer = 0f;
                HidePersistentMessage();
                return;
            }

            Vector3 rangeCheckPosition = playerRoot != null ? playerRoot.position : resolvedHead.position;
            bool inRange = rangeCollider == null || IsInsideRange(rangeCheckPosition);
            float lookAngle = Vector3.Angle(resolvedHead.forward, toTarget);
            LogState("checking", resolvedHead, resolvedTarget, inRange, lookAngle);

            if (!inRange || lookAngle > maxLookAngle)
            {
                lookTimer = 0f;
                HidePersistentMessage();
                return;
            }

            if (persistentMessageVisible)
            {
                return;
            }

            if (Time.time < nextShowTime)
            {
                return;
            }

            lookTimer += Time.deltaTime;
            if (lookTimer < lookHoldSeconds)
            {
                return;
            }

            FeedbackUIController targetUI = feedbackUI != null ? feedbackUI : FeedbackUIController.Instance;
            if (targetUI == null)
            {
                return;
            }

            bool isPersistent = duration < 0f;
            targetUI.Show(kind, title, message, isPersistent ? 0f : duration);
            shown?.Invoke();
            hasShown = true;
            persistentMessageVisible = isPersistent;
            lookTimer = 0f;
            nextShowTime = isPersistent ? 0f : Time.time + repeatDelay;
        }

        public void ResetPrompt()
        {
            hasShown = false;
            lookTimer = 0f;
            nextShowTime = 0f;
            persistentMessageVisible = false;
        }

        private bool IsInsideRange(Vector3 position)
        {
            Vector3 checkPosition = position;
            if (ignoreRangeHeight)
            {
                checkPosition.y = rangeCollider.bounds.center.y;
            }

            lastRangeCheckPosition = checkPosition;
            lastRangeClosestPoint = rangeCollider.ClosestPoint(checkPosition);
            lastRangeSqrDistance = (lastRangeClosestPoint - checkPosition).sqrMagnitude;
            return lastRangeSqrDistance < 0.0001f;
        }

        private void HidePersistentMessage()
        {
            if (!persistentMessageVisible)
            {
                return;
            }

            FeedbackUIController targetUI = feedbackUI != null ? feedbackUI : FeedbackUIController.Instance;
            if (targetUI != null)
            {
                targetUI.Hide();
            }

            persistentMessageVisible = false;
        }

        private void LogState(string state, Transform resolvedHead, Transform resolvedTarget, bool inRange, float lookAngle)
        {
            if (!debugLog || Time.time < nextDebugLogTime)
            {
                return;
            }

            nextDebugLogTime = Time.time + 0.5f;
            string rangeDetails = rangeCollider != null
                ? $", checkPos={lastRangeCheckPosition}, closest={lastRangeClosestPoint}, sqrDistance={lastRangeSqrDistance:0.0000}, rangeCenter={rangeCollider.bounds.center}, rangeSize={rangeCollider.bounds.size}, ignoreHeight={ignoreRangeHeight}"
                : "";

            Debug.Log($"[LookHintPrompt] {state}, head={(resolvedHead != null ? resolvedHead.name : "NULL")}, playerRoot={(playerRoot != null ? playerRoot.name : "NULL")}, target={(resolvedTarget != null ? resolvedTarget.name : "NULL")}, rangeCollider={(rangeCollider != null ? rangeCollider.name : "NULL")}, inRange={inRange}, angle={lookAngle:0.0}/{maxLookAngle}, timer={lookTimer:0.00}/{lookHoldSeconds}, cooldownLeft={Mathf.Max(0f, nextShowTime - Time.time):0.00}{rangeDetails}", this);
        }
    }
}
