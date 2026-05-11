using UnityEngine;

namespace EscapeRoom.UI
{
    [AddComponentMenu("Escape Room/UI/Feedback Event Trigger")]
    [DisallowMultipleComponent]
    public sealed class FeedbackEventTrigger : MonoBehaviour
    {
        [SerializeField] private FeedbackUIController feedbackUI;
        [SerializeField] private FeedbackUIController.FeedbackKind kind = FeedbackUIController.FeedbackKind.Info;
        [SerializeField] private string title = string.Empty;
        [TextArea(2, 4)]
        [SerializeField] private string message = "Something happened.";
        [SerializeField] private float duration = -1f;
        [SerializeField] private bool showOnEnable = false;

        private void OnEnable()
        {
            if (showOnEnable)
            {
                Show();
            }
        }

        public void Show()
        {
            FeedbackUIController target = ResolveFeedbackUI();
            if (target == null)
            {
                return;
            }

            target.Show(kind, title, message, duration);
        }

        public void ShowInfo(string nextMessage)
        {
            ShowWithKind(FeedbackUIController.FeedbackKind.Info, nextMessage);
        }

        public void ShowSuccess(string nextMessage)
        {
            ShowWithKind(FeedbackUIController.FeedbackKind.Success, nextMessage);
        }

        public void ShowWarning(string nextMessage)
        {
            ShowWithKind(FeedbackUIController.FeedbackKind.Warning, nextMessage);
        }

        public void ShowError(string nextMessage)
        {
            ShowWithKind(FeedbackUIController.FeedbackKind.Error, nextMessage);
        }

        public void ShowPuzzleSolved()
        {
            FeedbackUIController target = ResolveFeedbackUI();
            if (target != null)
            {
                target.ShowPuzzleSolved();
            }
        }

        public void ShowWrongPassword()
        {
            FeedbackUIController target = ResolveFeedbackUI();
            if (target != null)
            {
                target.ShowWrongPassword();
            }
        }

        public void ShowInputInstruction(string instruction)
        {
            FeedbackUIController target = ResolveFeedbackUI();
            if (target != null)
            {
                target.ShowInputInstruction(instruction);
            }
        }

        public void ShowPortalBlocked()
        {
            FeedbackUIController target = ResolveFeedbackUI();
            if (target != null)
            {
                target.ShowPortalBlocked();
            }
        }

        public void Hide()
        {
            FeedbackUIController target = ResolveFeedbackUI();
            if (target != null)
            {
                target.Hide();
            }
        }

        private void ShowWithKind(FeedbackUIController.FeedbackKind nextKind, string nextMessage)
        {
            FeedbackUIController target = ResolveFeedbackUI();
            if (target == null)
            {
                return;
            }

            target.Show(nextKind, title, nextMessage, duration);
        }

        private FeedbackUIController ResolveFeedbackUI()
        {
            return feedbackUI != null ? feedbackUI : FeedbackUIController.Instance;
        }
    }
}
