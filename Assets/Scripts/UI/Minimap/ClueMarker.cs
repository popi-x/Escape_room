using UnityEngine;
using EscapeRoom.UI;

[AddComponentMenu("Escape Room/Markers/Clue Marker")]
[DisallowMultipleComponent]
public class ClueMarker : MonoBehaviour
{
    [SerializeField] private string markerId;
    [SerializeField] private Transform markerCenter;

    [SerializeField] private ClueMarkerState currentState = ClueMarkerState.Undiscovered;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(markerId))
            markerId = gameObject.name;

#if UNITY_EDITOR
        if (Application.isPlaying && MarkerManager.Instance != null)
        {
            Vector3 pos = markerCenter != null ? markerCenter.position : transform.position;
            Debug.Log("Calling MarkerManager");
            MarkerManager.Instance.SetClueState(markerId, pos, currentState);
        }
#endif
    }

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(markerId))
            markerId = gameObject.name;
    }

    public void SetState(ClueMarkerState newState)
    {
        if (newState == currentState || MarkerManager.Instance == null) return;
        currentState = newState;
        Vector3 pos = markerCenter != null ? markerCenter.position : transform.position;
        MarkerManager.Instance.SetClueState(markerId, pos, currentState);
    }

}
