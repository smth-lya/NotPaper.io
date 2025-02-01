using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameShared.Commands.ServerToClient
{
    public sealed class PlayerJoinCommand : ServerToClientCommand
    {
        private static readonly Dictionary<string, int> _fieldOffsets = new()
        {
            { "PlayerId", 1 }
        };

        public override ServerToClientEvent CommandType => ServerToClientEvent.PLAYER_JOIN;
        public override int PacketSize => sizeof(byte) + sizeof(int);

        public int PlayerId { get; private set; }

        // 🔥 Описание структуры пакета

        public PlayerJoinCommand() { }

        public PlayerJoinCommand(int playerId)
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
            Debug.Log($"Игрок {PlayerId} присоединился к игре!");

            // 🔥 Здесь можно обновить список игроков в Unity

            return Task.CompletedTask;
        }
    }
}