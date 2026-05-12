using UnityEngine;
using TMPro;

public class PadlockManager : MonoBehaviour
{
    public enum InputMode { Trigger, Hover }

    [Header("Display")]
    public TextMeshPro[] digitDisplays;
    public TextMeshPro statusText;

    [Header("Settings")]
    public string unlockCode = "1234";
    public InputMode inputMode = InputMode.Trigger;

    [Header("Door")]
    public GameObject door;
    public Vector3 doorOpenOffset = new Vector3(0, 3f, 0); // moves up by default
    public float doorSlideSpeed = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonPressClip;
    public AudioClip successClip;
    public AudioClip failureClip;
    [Range(0f, 1f)] public float audioVolume = 1f;

    private string currentInput = "";
    private bool isUnlocked = false;
    private Vector3 doorClosedPos;
    private Vector3 doorOpenPos;
    private bool doorMoving = false;

    void Start()
    {
        EnsureAudioSource();
        UpdateDisplay();
        statusText.text = "";

        if (door != null)
        {
            doorClosedPos = door.transform.position;
            doorOpenPos = doorClosedPos + doorOpenOffset;
        }
    }

    void Update()
    {
        if (doorMoving && door != null)
        {
            door.transform.position = Vector3.MoveTowards(
                door.transform.position,
                doorOpenPos,
                doorSlideSpeed * Time.deltaTime
            );

            if (Vector3.Distance(door.transform.position, doorOpenPos) < 0.01f)
            {
                door.transform.position = doorOpenPos;
                doorMoving = false;
            }
        }
    }

    public void OnDigitPressed(int digit)
    {
        if (isUnlocked) return;
        Debug.Log($"[PadlockManager] OnDigitPressed digit={digit}, currentInput={currentInput}", this);
        if (currentInput.Length >= 4) return;
        currentInput += digit.ToString();
        UpdateDisplay();
        if (currentInput.Length == 4) CheckCode();
    }

    public void OnClearPressed()
    {
        if (isUnlocked) return;
        currentInput = "";
        UpdateDisplay();
        statusText.text = "";
    }

    public void OnBackspacePressed()
    {
        if (isUnlocked) return;
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
            isUnlocked = true;
            statusText.text = "UNLOCKED!";
            statusText.color = Color.green;
            PlayOneShot(successClip);
            OpenDoor();
        }
        else
        {
            statusText.text = "WRONG CODE";
            statusText.color = Color.red;
            PlayOneShot(failureClip);
            Invoke(nameof(OnClearPressed), 1f);
        }
    }

    void OpenDoor()
    {
        if (door == null)
        {
            Debug.LogWarning("[PadlockManager] No door assigned");
            return;
        }
        doorMoving = true;
        Debug.Log("[PadlockManager] Door opening");
    }

    public void PlayButtonPressSound()
    {
        PlayOneShot(buttonPressClip);
    }

    void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;
        EnsureAudioSource();
        audioSource.PlayOneShot(clip, audioVolume);
    }

    void EnsureAudioSource()
    {
        if (audioSource != null) return;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
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
}