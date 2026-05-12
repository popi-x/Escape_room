using UnityEngine;
using EscapeRoom.UI;

[AddComponentMenu("Escape Room/Markers/Puzzle Marker")]
[DisallowMultipleComponent]
public class PuzzleMarker : MonoBehaviour
{
    [SerializeField] private string markerId;
    [SerializeField] private Transform markerCenter;

    [SerializeField] private PuzzleMarkerState currentState = PuzzleMarkerState.Undiscovered;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(markerId))
            markerId = gameObject.name;

#if UNITY_EDITOR
        if (Application.isPlaying && MarkerManager.Instance != null)
        {
            Vector3 pos = markerCenter != null ? markerCenter.position : transform.position;
            MarkerManager.Instance.SetPuzzleState(markerId, pos, currentState);
        }
#endif
    }

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(markerId))
            markerId = gameObject.name;
    }

    public void SetState(PuzzleMarkerState newState)
    {
        if (newState == currentState || MarkerManager.Instance == null) return;
        currentState = newState;
        Vector3 pos = markerCenter != null ? markerCenter.position : transform.position;
        MarkerManager.Instance.SetPuzzleState(markerId, pos, currentState);
    }
}
