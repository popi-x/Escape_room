using System.Collections;
using EscapeRoom.Progression;
using EscapeRoom.Rooms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoom.UI
{
    [AddComponentMenu("Escape Room/UI/Game Session Controller")]
    [DefaultExecutionOrder(-190)]
    [DisallowMultipleComponent]
    public sealed class GameSessionController : MonoBehaviour
    {
        [Header("Session")]
        [SerializeField] private float gameDurationSeconds = 480f;
        [SerializeField] private bool resetProgressOnGameStart = true;
        [SerializeField] private bool resetProgressOnGameEnd = true;
        [SerializeField] private bool reloadActiveSceneOnEnd = true;
        [SerializeField] private float resultPopupSeconds = 2.5f;
        [SerializeField] private string designerTimerText = "Time Unlimited";

        [Header("References")]
        [SerializeField] private EscapeRoomTimer timer;
        [SerializeField] private MainMenuController mainMenu;
        [SerializeField] private FeedbackUIController feedbackUI;
        [SerializeField] private RoomFlowController roomFlowController;
        [SerializeField] private DesignerModeController designerModeController;

        [Header("Messages")]
        [SerializeField] private string failedTitle = "Game Failed";
        [SerializeField] private string failedBody = "Time is up.";
        [SerializeField] private string successTitle = "Success";
        [SerializeField] private string successBody = "You escaped.";

        private Coroutine endRoutine;
        private bool gameActive;
        private bool designerModeActive;

        public static GameSessionController Instance { get; private set; }

        public bool IsGameActive => gameActive;
        public bool IsDesignerModeActive => designerModeActive;
        public float GameDurationSeconds => gameDurationSeconds;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            ResolveReferences();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            ResolveReferences();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Update()
        {
            if (gameActive && timer != null && !timer.IsRunning && timer.CurrentSeconds <= 0f)
            {
                FailGame();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Reset()
        {
            ResolveReferences();
        }

        public void StartGame()
        {
            ResolveReferences();

            if (endRoutine != null)
            {
                StopCoroutine(endRoutine);
                endRoutine = null;
            }

            gameActive = true;
            designerModeActive = false;

            if (designerModeController != null)
            {
                designerModeController.EndDesignerMode();
            }

            if (resetProgressOnGameStart && ProgressState.Instance != null)
            {
                ProgressState.Instance.ResetToInitialFlags();
            }

            if (feedbackUI != null)
            {
                feedbackUI.Hide();
            }

            if (timer != null)
            {
                timer.StartTimer(gameDurationSeconds);
            }
        }

        public void StartDesignerMode()
        {
            ResolveReferences();

            if (endRoutine != null)
            {
                StopCoroutine(endRoutine);
                endRoutine = null;
            }

            gameActive = false;
            designerModeActive = true;

            if (feedbackUI != null)
            {
                feedbackUI.Hide();
            }

            if (timer != null)
            {
                timer.ShowUnlimited(designerTimerText);
            }

            if (designerModeController != null)
            {
                designerModeController.BeginDesignerMode();
            }
        }

        public void EndDesignerMode()
        {
            designerModeActive = false;

            if (designerModeController != null)
            {
                designerModeController.EndDesignerMode();
            }

            if (timer != null)
            {
                timer.ResetTimer();
                timer.StopTimer();
            }
        }

        public void CompleteGame()
        {
            EndGame(true);
        }

        public void FailGame()
        {
            EndGame(false);
        }

        public void HandleTimerExpired()
        {
            if (gameActive)
            {
                FailGame();
            }
        }

        private void EndGame(bool success)
        {
            if (!gameActive && endRoutine != null)
            {
                return;
            }

            gameActive = false;
            designerModeActive = false;

            if (endRoutine != null)
            {
                StopCoroutine(endRoutine);
            }

            endRoutine = StartCoroutine(EndGameRoutine(success));
        }

        private IEnumerator EndGameRoutine(bool success)
        {
            ResolveReferences();

            if (timer != null)
            {
                timer.StopTimer();
            }

            if (feedbackUI != null)
            {
                feedbackUI.Show(
                    success ? FeedbackUIController.FeedbackKind.Success : FeedbackUIController.FeedbackKind.Error,
                    success ? successTitle : failedTitle,
                    success ? successBody : failedBody,
                    resultPopupSeconds,
                    true);
            }

            if (resultPopupSeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(resultPopupSeconds);
            }

            ResetGameToMenu();
            endRoutine = null;
        }

        private void ResetGameToMenu()
        {
            if (resetProgressOnGameEnd && ProgressState.Instance != null)
            {
                ProgressState.Instance.ResetToInitialFlags();
            }

            if (timer != null)
            {
                timer.ResetTimer();
                timer.StopTimer();
            }

            if (designerModeController != null)
            {
                designerModeController.EndDesignerMode();
            }

            if (reloadActiveSceneOnEnd)
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene.IsValid())
                {
                    SceneManager.LoadScene(activeScene.name);
                    return;
                }
            }

            if (roomFlowController != null && !string.IsNullOrWhiteSpace(roomFlowController.CurrentSceneName))
            {
                roomFlowController.ReloadCurrentRoom();
            }

            ShowMenuAfterReset();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (endRoutine == null && !gameActive && !designerModeActive)
            {
                ResolveReferences();
                ShowMenuAfterReset();
            }
        }

        private void ShowMenuAfterReset()
        {
            ResolveReferences();

            if (mainMenu != null)
            {
                mainMenu.ShowMenu();
            }

            if (feedbackUI != null)
            {
                feedbackUI.Hide();
                feedbackUI.ClearTimer();
            }
        }

        private void ResolveReferences()
        {
            if (timer == null)
            {
                timer = FindFirstObjectByType<EscapeRoomTimer>();
            }

            if (mainMenu == null)
            {
                mainMenu = FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
            }

            if (feedbackUI == null)
            {
                feedbackUI = FeedbackUIController.Instance != null
                    ? FeedbackUIController.Instance
                    : FindFirstObjectByType<FeedbackUIController>();
            }

            if (roomFlowController == null)
            {
                roomFlowController = RoomFlowController.Instance != null
                    ? RoomFlowController.Instance
                    : FindFirstObjectByType<RoomFlowController>();
            }

            if (designerModeController == null)
            {
                designerModeController = FindFirstObjectByType<DesignerModeController>(FindObjectsInactive.Include);
            }
        }
    }
}
