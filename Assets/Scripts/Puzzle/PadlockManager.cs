using UnityEngine;
using TMPro;

public class PadlockManager : MonoBehaviour
{
    public enum InputMode { Trigger, Hover }

    [Header("Display")]
    public TextMeshPro[] digitDisplays; // 4 slots, assign in Inspector
    public TextMeshPro statusText;

    [Header("Settings")]
    public string unlockCode = "1234";
    public InputMode inputMode = InputMode.Trigger;

    private string currentInput = "";

    // Called by each digit button
    public void OnDigitPressed(int digit)
    {
        Debug.Log($"[PadlockManager] OnDigitPressed digit={digit}, inputMode={inputMode}, currentInput={currentInput}", this);
        if (currentInput.Length >= 4) return;
        statusText.text = "received input: " + digit; // Debug log for input
        currentInput += digit.ToString();
        UpdateDisplay();

        // Auto-check when 4 digits entered
        if (currentInput.Length == 4)
            CheckCode();
    }

    // public void OnEnterPressed()
    // {
    //     if (currentInput.Length == 0) return;
    //     CheckCode();
    // }

    public void OnClearPressed()
    {
        currentInput = "";
        UpdateDisplay();
        statusText.text = "";
    }

    public void OnBackspacePressed()
    {
        if (currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdateDisplay();
            statusText.text = "";
        }
    }

    void CheckCode()
    {
        if (currentInput == unlockCode)
        {
            statusText.text = "UNLOCKED!";
            statusText.color = Color.green;
            // Trigger unlock animation here if needed
        }
        else
        {
            statusText.text = "WRONG CODE";
            statusText.color = Color.red;
            // Clear after short delay
            Invoke(nameof(OnClearPressed), 1f);
        }
    }

    void UpdateDisplay()
    {
        for (int i = 0; i < digitDisplays.Length; i++)
        {
            if (i < currentInput.Length)
                digitDisplays[i].text = currentInput[i].ToString();
            else
                digitDisplays[i].text = "_";
        }
    }

    void Start()
    {
        UpdateDisplay();
        statusText.text = "";
    }
}
