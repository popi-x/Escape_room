using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class BuzzWireKey : MonoBehaviour
{
    public BuzzWireManager gameManager;

    [Header("Feedback")]
    // public AudioSource buzzSound;
    public float cooldown = 0.5f; // prevent rapid repeated triggers

    private float lastBuzzTime = -999f;
    private bool isHeld = false;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(_ => isHeld = true);
        grabInteractable.selectExited.AddListener(_ => isHeld = false);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Key triggered: " + other.name + other.tag);
        if (!other.CompareTag("Wire")) return;
        if (!isHeld) return;
        if (Time.time - lastBuzzTime < cooldown) return;

        lastBuzzTime = Time.time;
        gameManager.OnBuzz();

        // if (buzzSound != null) buzzSound.Play();
    }

    void OnTriggerEnterEnd(Collider other) // end zone
    {
        if (!other.CompareTag("WireEnd")) return;
        if (!isHeld) return;
        gameManager.OnWin();
    }
}