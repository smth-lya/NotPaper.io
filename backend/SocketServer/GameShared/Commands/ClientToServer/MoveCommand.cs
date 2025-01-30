using System.Net.Sockets;
using GameShared.Commands.ServerToClient;
using GameShared.Interfaces;

namespace GameShared.Commands.ClientToServer;

public class MoveCommand : IClientToServerCommandHandler
{
    public ClientToServerEvent CommandType => ClientToServerEvent.MOVE;
    public int PacketSize => 6; // 🔥 Размер команды теперь фиксированный

    public int PlayerId { get; private set; }
    public int Direction { get; private set; }

    // 🔥 Теперь словарь `FieldOffsets` статический, чтобы доступ был отовсюду
    public static Dictionary<string, int> FieldOffsets { get; protected set; } = new()
    {
        { "PlayerId", 1 }, // ID игрока с 1-го байта
        { "Direction", 5 }  // Направление с 5-го байта
    };

    public void ParseFromBytes(byte[] data)
    {
        PlayerId = BitConverter.ToInt32(data, FieldOffsets["PlayerId"]);
        Direction = data[FieldOffsets["Direction"]];
    }

    public byte[] ToBytes()
    {
        byte[] result = new byte[PacketSize];
        result[0] = (byte)CommandType;
        BitConverter.GetBytes(PlayerId).CopyTo(result, FieldOffsets["PlayerId"]);
        result[FieldOffsets["Direction"]] = (byte)Direction;
        return result;
    }

    public async Task Execute(PaperServer server, Socket clientSocket)
    {
        Console.WriteLine($"Игрок {PlayerId} сменил направление на {Direction}");

        // if (server.Players.TryGetValue(PlayerId, out var player))
        // {
        //     player.CurrentDirection = (Direction)Direction;
        // }

        byte[] response = new PlayerMoveCommand(PlayerId, Direction).ToBytes();
        await server.Broadcast(response);
    }
}