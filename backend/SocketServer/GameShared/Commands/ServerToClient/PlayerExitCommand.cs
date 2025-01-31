using GameShared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameShared.Commands.ServerToClient
{
    public sealed class PlayerExitCommand : ServerToClientCommand
    {
        private static readonly Dictionary<string, int> _fieldOffsets = new()
        {
            { "PlayerId", 1 }
        };

        public override ServerToClientEvent CommandType => ServerToClientEvent.PLAYER_EXIT;
        public override int PacketSize => sizeof(byte) + sizeof(int);

        public int PlayerId { get; private set; }


        public PlayerExitCommand() { }

        public PlayerExitCommand(int playerId)
        {
            PlayerId = playerId;
        }

        public override void ParseFromBytes(byte[] data)
        {
            PlayerId = BitConverter.ToInt32(data, _fieldOffsets["PlayerId"]);
        }

        public override byte[] ToBytes()
        {
            byte[] result = new byte[PacketSize];
            result[0] = (byte)CommandType;

            BitConverter.GetBytes(PlayerId).CopyTo(result, _fieldOffsets["PlayerId"]);
            
            return result;
        }

        public override Task ExecuteAsync(PaperClient client)
        {
            Console.WriteLine($"Игрок {PlayerId} покинул игру!");
            // В Unity можно обновить список игроков

            return Task.CompletedTask;
        }
    }
}