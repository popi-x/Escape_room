using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using EscapeRoom.UI;

public class GameSuccessController : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        restartButton.onClick.AddListener(Restart);
        exitButton.onClick.AddListener(Exit);
    }

    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(Restart);
        exitButton.onClick.RemoveListener(Exit);
    }

    private void Restart()
    {
        if (GameSessionController.Instance != null)
        {
            GameSessionController.Instance.RestartToInitialMenu();
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
