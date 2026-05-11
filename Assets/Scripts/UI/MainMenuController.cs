using EscapeRoom.Rooms;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EscapeRoom.UI
{
    [AddComponentMenu("Escape Room/UI/Main Menu Controller")]
    [DisallowMultipleComponent]
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private RoomFlowController roomFlowController;
        [SerializeField] private string playerModeSceneName = "Room_1";
        [SerializeField] private FeedbackUIController feedbackUI;
        [SerializeField] private GameSessionController gameSessionController;
        [SerializeField] private string designerModeMessage = "Designer Mode is not set up yet.";

        private void Reset()
        {
            menuRoot = gameObject;
            roomFlowController = FindFirstObjectByType<RoomFlowController>();
            feedbackUI = FindFirstObjectByType<FeedbackUIController>();
            gameSessionController = FindFirstObjectByType<GameSessionController>();
        }

        public void StartPlayerMode()
        {
            GameSessionController session = ResolveGameSessionController();
            if (session != null)
            {
                session.StartGame();
            }
            else
            {
                RoomFlowController flow = ResolveRoomFlowController();
                if (flow != null && !string.IsNullOrWhiteSpace(playerModeSceneName))
                {
                    flow.RequestRoomChange(playerModeSceneName);
                }
            }

            SetMenuVisible(false);
        }

        public void OpenDesignerMode()
        {
            FeedbackUIController feedback = ResolveFeedbackUI();
            if (feedback != null)
            {
                feedback.Show(
                    FeedbackUIController.FeedbackKind.Info,
                    "Designer Mode",
                    designerModeMessage);
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void ShowMenu()
        {
            SetMenuVisible(true);
        }

        public void HideMenu()
        {
            SetMenuVisible(false);
        }

        private void SetMenuVisible(bool visible)
        {
            GameObject target = menuRoot != null ? menuRoot : gameObject;
            target.SetActive(visible);
        }

        private RoomFlowController ResolveRoomFlowController()
        {
            if (roomFlowController == null)
            {
                roomFlowController = RoomFlowController.Instance;
            }

            return roomFlowController;
        }

        private FeedbackUIController ResolveFeedbackUI()
        {
            if (feedbackUI == null)
            {
                feedbackUI = FeedbackUIController.Instance;
            }

            return feedbackUI;
        }

        private GameSessionController ResolveGameSessionController()
        {
            if (gameSessionController == null)
            {
                gameSessionController = GameSessionController.Instance != null
                    ? GameSessionController.Instance
                    : FindFirstObjectByType<GameSessionController>();
            }

            return gameSessionController;
        }
    }
}
