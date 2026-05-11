using UnityEngine;

namespace EscapeRoom.Rooms
{
    /// <summary>
    /// 一个房间场景的基础信息。
    /// 每个房间 scene 里放一个即可。
    /// </summary>
    [AddComponentMenu("Escape Room/Rooms/Room Scene Info")]
    [DisallowMultipleComponent]
    public sealed class RoomSceneInfo : MonoBehaviour
    {
        [Tooltip("显示在 UI 里的房间名称。留空时使用场景名。")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("房间在 minimap 上使用的网格坐标。")]
        [SerializeField] private Vector2Int minimapCoordinate = Vector2Int.zero;

        [Tooltip("房间在 minimap 上使用的颜色。")]
        [SerializeField] private Color minimapColor = Color.white;

        public string SceneName => gameObject.scene.name;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? SceneName : displayName;
        public Vector2Int MinimapCoordinate => minimapCoordinate;
        public Color MinimapColor => minimapColor;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = string.IsNullOrWhiteSpace(gameObject.scene.name) ? gameObject.name : gameObject.scene.name;
            }
        }
    }
}
