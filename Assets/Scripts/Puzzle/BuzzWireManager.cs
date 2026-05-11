using UnityEngine;
using TMPro;

public class BuzzWireManager : MonoBehaviour
{
    public TextMeshPro statusText;
    public TextMeshPro strikeText;
    public Transform keyStartPosition; // where key resets to on buzz
    public BuzzWireKey key;

    [Header("Settings")]
    public int maxStrikes = 3;

    private int strikes = 0;
    private bool gameOver = false;

    void Start()
    {
        UpdateUI();
    }

    public void OnBuzz()
    {
        if (gameOver) return;

        strikes++;
        Debug.Log("BUZZ! Strikes: " + strikes);

        // Haptic feedback on both controllers
        // TriggerHaptics();

        if (strikes >= maxStrikes)
        {
            GameOver();
        }
        else
        {
            statusText.text = "BUZZ!";
            Invoke(nameof(ClearStatus), 1f);
        }

        UpdateUI();
    }

    public void OnWin()
    {
        if (gameOver) return;
        gameOver = true;
        statusText.text = "YOU WIN!";
        statusText.color = Color.green;
        Debug.Log("Player won!");
    }

    void GameOver()
    {
        gameOver = true;
        statusText.text = "GAME OVER";
        statusText.color = Color.red;

        // Reset key to start
        key.transform.position = keyStartPosition.position;
        key.transform.rotation = keyStartPosition.rotation;
    }

    // void TriggerHaptics()
    // {
    //     OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.RTouch);
    //     OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.LTouch);
    //     Invoke(nameof(StopHaptics), 0.3f);
    // }

    // void StopHaptics()
    // {
    //     OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    //     OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    // }

    void ClearStatus() => statusText.text = "";

    void UpdateUI()
    {
        strikeText.text = $"Strikes: {strikes}/{maxStrikes}";
    }
}