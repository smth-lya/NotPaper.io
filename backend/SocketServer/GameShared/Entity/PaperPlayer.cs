using System.Net.Sockets;
using System.Numerics;

namespace GameShared.Entity
{
    public class PaperPlayer : BasePlayer
    {
        public Socket Socket { get; }

        public PaperPlayer(int id, Socket socket)
        {
            Id = id;
            Socket = socket;

            Position = Vector3.Zero;
            Direction = Vector3.One;
        }
    }
}