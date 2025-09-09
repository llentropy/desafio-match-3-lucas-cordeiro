using System;

namespace Gazeus.DesafioMatch3.Models
{
    public class Tile
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public float SpawnTimestamp { get; set; }
        public float BlockedStatusDuration { get; set; }
        public bool IsBlocked { get; set; }
    }
}
