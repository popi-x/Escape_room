using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoom.UI
{
    public enum PuzzleMarkerState { Undiscovered, Locked, Unlocked }
    public enum ClueMarkerState { Undiscovered, Partial, Full }

    [AddComponentMenu("Escape Room/UI/Marker Manager")]
    [DisallowMultipleComponent]
    public sealed class MarkerManager : MonoBehaviour
    {
        [Tooltip("Y world position for all marker icons (match the value in NewMinimapController).")]
        [SerializeField] private float markerHeight = 0.1f;

        [Header("Puzzle Sprites")]
        [SerializeField] private Sprite puzzleLockedSprite;
        [SerializeField] private Sprite puzzleUnlockedSprite;

        [Header("Clue Sprites")]
        [SerializeField] private Sprite cluePartialSprite;
        [SerializeField] private Sprite clueFullSprite;

        [Header("Icon")]
        [SerializeField] private Transform miniMapUI;
        [SerializeField] private float iconScale = 0.5f;

        public static MarkerManager Instance { get; private set; }

        private readonly Dictionary<string, GameObject> activeIcons = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            foreach (KeyValuePair<string, GameObject> pair in activeIcons)
            {
                if (pair.Value != null) Destroy(pair.Value);
            }
            activeIcons.Clear();
        }

        public void SetPuzzleState(string id, Vector3 worldPos, PuzzleMarkerState state)
        {
            if (state == PuzzleMarkerState.Undiscovered) { HideIcon(id); return; }
            Sprite sprite = state == PuzzleMarkerState.Locked ? puzzleLockedSprite : puzzleUnlockedSprite;
            ShowIcon(id, worldPos, sprite);
        }

        public void SetClueState(string id, Vector3 worldPos, ClueMarkerState state)
        {
            if (state == ClueMarkerState.Undiscovered) { HideIcon(id); return; }
            Sprite sprite = state == ClueMarkerState.Partial ? cluePartialSprite : clueFullSprite;
            Debug.Log("[MarkerManager] Show Icon");
            ShowIcon(id, worldPos, sprite);
        }

        private void ShowIcon(string id, Vector3 worldPos, Sprite sprite)
        {
            Vector3 flatPos = new Vector3(worldPos.x, worldPos.y, worldPos.z);

            if (activeIcons.TryGetValue(id, out GameObject existing) && existing != null)
            {
                existing.transform.position = flatPos;
                existing.GetComponent<SpriteRenderer>().sprite = sprite;
                existing.SetActive(true);
                return;
            }

            GameObject icon = new GameObject($"MinimapIcon_{id}");
            icon.transform.position = flatPos;
            icon.transform.rotation = Quaternion.Euler(90, 0, 0);
            icon.transform.localScale = Vector3.one * iconScale;
            icon.transform.parent = miniMapUI;

            Debug.Log($"[MarkerManager] Icon initialized with name {icon}");

            int minimapLayer = LayerMask.NameToLayer("Minimap Only");
            if (minimapLayer >= 0) icon.layer = minimapLayer;

            icon.AddComponent<SpriteRenderer>().sprite = sprite;
            activeIcons[id] = icon;
        }

        private void HideIcon(string id)
        {
            if (activeIcons.TryGetValue(id, out GameObject icon) && icon != null)
                icon.SetActive(false);
        }
    }
}
