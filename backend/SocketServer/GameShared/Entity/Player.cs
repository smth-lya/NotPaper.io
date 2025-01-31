using System.Net.Sockets;
using System.Numerics;

namespace GameShared.Entity;

public class Player
{
    public int Id { get; }
    public Socket Socket { get; }
    public float X { get; set; }
    public float Z { get; set; }
    public Vector2 Direction { get; set; }
    public int CurrentDirection { get; set; }

    public Player(int id, Socket socket)
    {
        Id = id;
        Socket = socket;
        X = 0;
        Z = 0;
        CurrentDirection = 0;
    }
}