using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public struct AddedTileInfo
    {
        public Vector2Int Position { get; set; }
        public int Type { get; set; }
        public float SpawnTimestamp { get; set; }
        public bool IsBlocked { get; set; }
        public float BlockedStatusDuration { get; set; }
    }
}
