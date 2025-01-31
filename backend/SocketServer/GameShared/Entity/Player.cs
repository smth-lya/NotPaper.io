using System.Net.Sockets;

namespace GameShared.Entity;

public class Player
{
    public int Id { get; }
    public Socket Socket { get; }
    public float X { get; set; }
    public float Y { get; set; }
    public int CurrentDirection { get; set; }

    public Player(int id, Socket socket)
    {
        Id = id;
        Socket = socket;
        X = 0;
        Y = 0;
        CurrentDirection = 0;
    }
}