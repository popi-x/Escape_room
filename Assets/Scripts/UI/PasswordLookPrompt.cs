using UnityEngine;

namespace EscapeRoom.UI
{
    [AddComponentMenu("Escape Room/UI/Password Look Prompt")]
    public sealed class PasswordLookPrompt : MonoBehaviour
    {
        [SerializeField] private FeedbackUIController feedbackUI;
        [SerializeField] private Transform head;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform lookTarget;
        [SerializeField] private Collider rangeCollider;
        [SerializeField] private bool ignoreRangeHeight = true;
        [SerializeField] private float maxLookAngle = 25f;
        [SerializeField] private float lookHoldSeconds = 1f;
        [SerializeField] private string title = "Password";
        [SerializeField] private string message = "Use Trigger to enter the password.";
        [SerializeField] private float duration = 3f;
        [SerializeField] private float repeatDelay = 8f;
        [SerializeField] private bool debugLog = true;

        private float nextShowTime;
        private float lookTimer;
        private float nextDebugLogTime;
        private Vector3 lastRangeCheckPosition;
        private Vector3 lastRangeClosestPoint;
        private float lastRangeSqrDistance;

        private void Reset()
        {
            lookTarget = transform;
        }

        private void Update()
        {
            Transform resolvedHead = head != null ? head : Camera.main != null ? Camera.main.transform : null;
            Transform resolvedTarget = lookTarget != null ? lookTarget : transform;
            if (resolvedHead == null || resolvedTarget == null || Time.time < nextShowTime)
            {
                LogState("missing head/target or cooldown", resolvedHead, resolvedTarget, false, 0f);
                return;
            }

            Vector3 toTarget = resolvedTarget.position - resolvedHead.position;
            Vector3 rangeCheckPosition = playerRoot != null ? playerRoot.position : resolvedHead.position;
            bool inRange = rangeCollider == null || IsInsideRange(rangeCheckPosition);
            float lookAngle = Vector3.Angle(resolvedHead.forward, toTarget);
            LogState("checking", resolvedHead, resolvedTarget, inRange, lookAngle);

            if (!inRange)
            {
                lookTimer = 0f;
                return;
            }

            if (lookAngle > maxLookAngle)
            {
                lookTimer = 0f;
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

            bool didShow = targetUI.Show(FeedbackUIController.FeedbackKind.Info, title, message, duration);
            if (didShow)
            {
                Debug.Log($"[PasswordLookPrompt] SHOW title={title}, message={message}", this);
                lookTimer = 0f;
                nextShowTime = Time.time + repeatDelay;
            }
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

            // Debug.Log($"[PasswordLookPrompt] {state}, head={(resolvedHead != null ? resolvedHead.name : "NULL")}, playerRoot={(playerRoot != null ? playerRoot.name : "NULL")}, target={(resolvedTarget != null ? resolvedTarget.name : "NULL")}, rangeCollider={(rangeCollider != null ? rangeCollider.name : "NULL")}, inRange={inRange}, angle={lookAngle:0.0}/{maxLookAngle}, timer={lookTimer:0.00}/{lookHoldSeconds}, cooldownLeft={Mathf.Max(0f, nextShowTime - Time.time):0.00}{rangeDetails}", this);
        }
    }
}
