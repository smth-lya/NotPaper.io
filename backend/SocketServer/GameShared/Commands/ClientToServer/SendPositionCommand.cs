using System.Net.Sockets;
using GameShared.Commands.ServerToClient;
using GameShared.Entity;
using GameShared.Interfaces;

namespace GameShared.Commands.ClientToServer;

public class SendPositionCommand : IClientToServerCommandHandler
{
    public ClientToServerEvent CommandType => ClientToServerEvent.SEND_POSITION;
    public int PacketSize => 13; // 1 байт - команда, 4 байта - PlayerId, 4 байта - X, 4 байта - Y

    public int PlayerId { get; private set; }
    public float X { get; private set; }
    public float Y { get; private set; }

    public static Dictionary<string, int> FieldOffsets { get; protected set; } = new()
    {
        { "PlayerId", 1 },
        { "X", 5 },
        { "Y", 9 }
    };

    public SendPositionCommand() { }

    public SendPositionCommand(int playerId, float x, float y)
    {
        PlayerId = playerId;
        X = x;
        Y = y;
    }

    public void ParseFromBytes(byte[] data)
    {
        PlayerId = BitConverter.ToInt32(data, FieldOffsets["PlayerId"]);
        X = BitConverter.ToSingle(data, FieldOffsets["X"]);
        Y = BitConverter.ToSingle(data, FieldOffsets["Y"]);
    }

    public byte[] ToBytes()
    {
        byte[] result = new byte[PacketSize];
        result[0] = (byte)CommandType;
        BitConverter.GetBytes(PlayerId).CopyTo(result, FieldOffsets["PlayerId"]);
        BitConverter.GetBytes(X).CopyTo(result, FieldOffsets["X"]);
        BitConverter.GetBytes(Y).CopyTo(result, FieldOffsets["Y"]);
        return result;
    }

    public async Task Execute(PaperServer server, Socket clientSocket)
    {
        Console.WriteLine($"Игрок {PlayerId} сообщил свою позицию: X={X}, Y={Y}");

        server.PlayerPositions[PlayerId] = (X, Y);

        // 🔥 Проверяем, прислали ли все игроки свои позиции
        if (server.PlayerPositions.Count == server.Players.Count)
        {
            Console.WriteLine("[Server] Все игроки отправили позиции, рассылаем `GameStateCommand`.");
            var gameStateCommand = new GameStateCommand(
                server.Players.Values.Select(p => (p.Id, server.PlayerPositions[p.Id].X, server.PlayerPositions[p.Id].Y)).ToList()
            );
            await server.Broadcast(gameStateCommand.ToBytes());
        }
    }
}
