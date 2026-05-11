using UnityEngine;


public class BuzzWireKey : MonoBehaviour
{
    [HideInInspector]
    public BuzzWireManager gameManager;
    [HideInInspector]
    public Collider winArea;

    public AudioSource buzzSound;
    public float cooldown = 0.5f;

    private float lastBuzzTime = -999f;
    private bool isHeld = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(_ => isHeld = true);
            grabInteractable.selectExited.AddListener(_ => isHeld = false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[BuzzWireKey] TriggerEnter: {other.gameObject.name} tag: {other.tag}");

        if (IsWinArea(other))
        {
            if (!isHeld) return;
            if (gameManager != null) gameManager.OnWin();
            return;
        }

        if (!other.CompareTag("Wire")) return;
        if (!isHeld) return;
        if (Time.time - lastBuzzTime < cooldown) return;

        lastBuzzTime = Time.time;

        if (buzzSound != null) buzzSound.Play();
        if (gameManager != null) gameManager.OnBuzz();
        // OnBuzz destroys this GameObject so nothing after this runs
    }

    private bool IsWinArea(Collider other)
    {
        return winArea != null && other == winArea;
    }

    // void OnTriggerEnter(Collider other) // end zone
    // {
    //     if (!other.CompareTag("WireEnd")) return;
    //     if (gameManager != null) gameManager.OnWin();
    // }
}

// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;


// public class BuzzWireKey : MonoBehaviour
// {
//     public BuzzWireManager gameManager;

//     [Header("Feedback")]
//     // public AudioSource buzzSound;
//     public float cooldown = 0.5f; // prevent rapid repeated triggers

//     private float lastBuzzTime = -999f;
//     private bool isHeld = false;

//     private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

//     void Start()
//     {
//         grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
//         grabInteractable.selectEntered.AddListener(_ => isHeld = true);
//         grabInteractable.selectExited.AddListener(_ => isHeld = false);
//     }

//     void OnTriggerEnter(Collider other)
//     {
//         Debug.Log($"[BuzzWireKey] TriggerEnter with: {other.gameObject.name} tag: {other.tag}");
//         if (!other.CompareTag("Wire")) return;
//         if (!isHeld) return;
//         if (Time.time - lastBuzzTime < cooldown) return;

//         lastBuzzTime = Time.time;
//         gameManager.OnBuzz();

//         // if (buzzSound != null) buzzSound.Play();
//     }

//     void OnTriggerEnterEnd(Collider other) // end zone
//     {
//         if (!other.CompareTag("WireEnd")) return;
//         if (!isHeld) return;
//         gameManager.OnWin();
//     }
// }
