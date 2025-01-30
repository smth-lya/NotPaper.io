using GameShared.Interfaces;

namespace GameShared.Commands.ServerToClient;

public class PlayerMoveCommand : IServerToClientCommandHandler
{
    public ServerToClientEvent CommandType => ServerToClientEvent.PLAYER_MOVE;
    public int PacketSize => 6;

    public int PlayerId { get; private set; }
    public int Direction { get; private set; }

    // 🔥 Словарь со структурами данных
    public static Dictionary<string, int> FieldOffsets { get; protected set; } = new()
    {
        { "PlayerId", 1 },
        { "Direction", 5 }
    };

    public PlayerMoveCommand() {}

    public PlayerMoveCommand(int playerId, int direction)
    {
        PlayerId = playerId;
        Direction = direction;
    }

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

    public async Task Execute(PaperClient client)
    {
        Console.WriteLine($"Игрок {PlayerId} передвинулся в направлении {Direction}");

        // if (client.Players.TryGetValue(PlayerId, out var player))
        // {
        //     player.CurrentDirection = (Direction)Direction;
        // }
    }
}