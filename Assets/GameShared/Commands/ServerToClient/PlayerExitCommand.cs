using GameShared;
using GameShared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameShared.Commands.ServerToClient
{
    public class PlayerExitCommand : IServerToClientCommandHandler
    {
        public ServerToClientEvent CommandType => ServerToClientEvent.PLAYER_EXIT;
        public int PacketSize => 5;

        public int PlayerId { get; private set; }

        public static Dictionary<string, int> FieldOffsets { get; protected set; } = new()
        {
            { "PlayerId", 1 }
        };

        public PlayerExitCommand() { }

        public PlayerExitCommand(int playerId)
        {
            PlayerId = playerId;
        }

        public void ParseFromBytes(byte[] data)
        {
            PlayerId = BitConverter.ToInt32(data, FieldOffsets["PlayerId"]);
        }

        public byte[] ToBytes()
        {
            byte[] result = new byte[PacketSize];
            result[0] = (byte)CommandType;
            BitConverter.GetBytes(PlayerId).CopyTo(result, FieldOffsets["PlayerId"]);
            return result;
        }

        public async Task Execute(PaperClient client)
        {
            Console.WriteLine($"Игрок {PlayerId} покинул игру!");
            // В Unity можно обновить список игроков
        }
    }
}