using System.Net.Sockets;

namespace GameShared.Entity
{

    public class PlayerNet
    {
        public int Id { get; }
        public Socket Socket { get; }
        public int X { get; set; }
        public int Y { get; set; }
        public int CurrentDirection { get; set; }

        public PlayerNet(int id, Socket socket)
        {
            Id = id;
            Socket = socket;
            X = 0;
            Y = 0;
            CurrentDirection = 0;
        }
    }
}