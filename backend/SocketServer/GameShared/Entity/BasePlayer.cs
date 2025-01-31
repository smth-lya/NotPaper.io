using System.Numerics;

namespace GameShared.Entity
{

    public class BasePlayer
    {
        public int Id { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
    }
}