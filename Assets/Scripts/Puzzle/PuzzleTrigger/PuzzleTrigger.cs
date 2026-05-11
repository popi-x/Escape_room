using UnityEngine;
using UnityEngine.InputSystem;
using EscapeRoom.UI;

// Attach to the puzzle/clue object alongside a ClueMarker or PuzzleMarker.
// Assign a child GameObject (with ProximityDetector + Sphere Collider IsTrigger) to Sphere Collider Object.
public class PuzzleTrigger : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject hintUI;

    [Header("Input")]
    [SerializeField] private InputActionProperty inspectAction;

    [Header("Proximity")]
    [SerializeField] private ProximityDetector sphereColliderObject;

    [Header("State on Inspect")]
    [SerializeField] private ClueMarkerState   clueStateOnInspect   = ClueMarkerState.Full;
    [SerializeField] private PuzzleMarkerState puzzleStateOnInspect = PuzzleMarkerState.Locked;

    private bool playerNearby = false;
    private bool hasInspected = false;

    private ClueMarker   clueMarker;
    private PuzzleMarker puzzleMarker;

    private void Awake()
    {
        clueMarker   = GetComponent<ClueMarker>();
        puzzleMarker = GetComponent<PuzzleMarker>();
    }

    private void OnEnable()
    {
        inspectAction.action.Enable();
        inspectAction.action.performed += OnInspect;

        if (sphereColliderObject != null)
        {
            sphereColliderObject.OnEnter += HandleEnter;
            sphereColliderObject.OnExit  += HandleExit;
        }
    }

    private void OnDisable()
    {
        inspectAction.action.performed -= OnInspect;
        inspectAction.action.Disable();

        if (sphereColliderObject != null)
        {
            sphereColliderObject.OnEnter -= HandleEnter;
            sphereColliderObject.OnExit  -= HandleExit;
        }
    }

    private void HandleEnter(Collider other)
    {
        if (!other.CompareTag("GameController") || hasInspected) return;
        playerNearby = true;
        hintUI?.SetActive(true);
    }

    private void HandleExit(Collider other)
    {
        if (!other.CompareTag("GameController")) return;
        playerNearby = false;
        hintUI?.SetActive(false);
    }

    private void OnInspect(InputAction.CallbackContext ctx)
    {
        if (!playerNearby || hasInspected) return;

        hasInspected = true;
        playerNearby = false;
        hintUI?.SetActive(false);

        if (clueMarker != null)
            clueMarker.SetState(clueStateOnInspect);
        else if (puzzleMarker != null)
            puzzleMarker.SetState(puzzleStateOnInspect);
    }
}
