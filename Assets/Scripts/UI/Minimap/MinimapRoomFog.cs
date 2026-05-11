using UnityEngine;

namespace EscapeRoom.UI
{
    /// <summary>
    /// Place this on a flat plane mesh that covers a room area on the MinimapOnly layer.
    /// The plane blocks the top-down minimap camera until the player's rig enters the area.
    /// </summary>
    [AddComponentMenu("Escape Room/UI/Minimap Room Fog")]
    [DisallowMultipleComponent]
    public sealed class MinimapRoomFog : MonoBehaviour
    {
        private Renderer fogRenderer;

        private void Awake()
        {
            fogRenderer = GetComponent<Renderer>();
        }

        /// <summary>
        /// Returns true if the given world XZ position falls within this fog plane's footprint.
        /// Y is ignored — only the horizontal area matters.
        /// </summary>
        public bool ContainsPosition(Vector3 worldPosition)
        {
            if (fogRenderer == null)
            {
                return false;
            }

            Bounds b = fogRenderer.bounds;
            return worldPosition.x >= b.min.x && worldPosition.x <= b.max.x
                && worldPosition.z >= b.min.z && worldPosition.z <= b.max.z;
        }

        public void Reveal() => gameObject.SetActive(false);
    }
}
