using System.Net.Sockets;
using UnityEngine;

namespace GameShared.Entity
{
    public class PaperPlayer : BasePlayer
    {
        public Socket Socket { get; }

        public PaperPlayer(int id, Socket socket)
        {
            Id = id;
            Socket = socket;

            Position = Vector3.zero;
            Direction = Vector3.one;
        }
    }
}