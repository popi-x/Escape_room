using UnityEngine;

namespace EscapeRoom.UI
{
    /// <summary>
    /// Top-down camera minimap controller.
    /// Moves a world-space player arrow and manages per-room fog-of-war planes.
    ///
    /// Setup:
    ///   1. Create a layer called "MinimapOnly".
    ///   2. Set the minimap camera's Culling Mask to include Default + MinimapOnly.
    ///   3. Set the main game camera's Culling Mask to EXCLUDE MinimapOnly.
    ///   4. Put fog planes and the player arrow prefab on the MinimapOnly layer.
    /// </summary>
    [AddComponentMenu("Escape Room/UI/Minimap Controller")]
    [DisallowMultipleComponent]
    public sealed class NewMinimapController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("XR rig root or player body. Auto-resolved from Camera.main if left blank.")]
        [SerializeField] private Transform playerRoot;

        [Tooltip("Player head/camera, used for facing direction. Auto-resolved from Camera.main if left blank.")]
        [SerializeField] private Transform playerHead;

        [Header("Player Arrow")]
        [Tooltip("3D arrow prefab placed in world space. Put it on the MinimapOnly layer.")]
        [SerializeField] private GameObject playerArrowPrefab;

        [Tooltip("Y world position for the player arrow (should be above room floor, below minimap camera).")]
        [SerializeField] private float markerHeight = 0.1f;

        [Header("Fog of War")]
        [Tooltip("All MinimapRoomFog planes in the scene. Each covers one room area.")]
        [SerializeField] private MinimapRoomFog roomFog;

        private Transform playerArrow;

        private void Start()
        {
            ResolvePlayerReferences();
            SpawnPlayerArrow();
        }

        private void Update()
        {
            ResolvePlayerReferences();
            UpdatePlayerArrow();
            CheckFogReveal();
        }

        private void OnDestroy()
        {
            if (playerArrow != null)
                Destroy(playerArrow.gameObject);
        }

        private void CheckFogReveal()
        {
            if (roomFog == null || !roomFog.gameObject.activeSelf || playerRoot == null)
                return;

            if (roomFog.ContainsPosition(playerRoot.position))
                roomFog.Reveal();
        }

        private void ResolvePlayerReferences()
        {
            if (playerHead == null && Camera.main != null)
                playerHead = Camera.main.transform;

            if (playerRoot == null && playerHead != null)
                playerRoot = playerHead.root;
        }

        private void SpawnPlayerArrow()
        {
            if (playerArrowPrefab == null || playerRoot == null)
                return;

            GameObject arrow = Instantiate(playerArrowPrefab, Vector3.zero, Quaternion.Euler(90f, -90f, 0f));
            arrow.name = "MinimapPlayerArrow";
            playerArrow = arrow.transform;
            UpdatePlayerArrow();
        }

        private void UpdatePlayerArrow()
        {
            if (playerArrow == null || playerRoot == null)
                return;

            Transform reference = playerHead ?? playerRoot;
            playerArrow.position = new Vector3(playerRoot.position.x, markerHeight, playerRoot.position.z);
            playerArrow.rotation = Quaternion.Euler(90f, reference.eulerAngles.y - 90, 0f);
        }
    }
}
