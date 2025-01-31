using GameShared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameShared.Commands.ServerToClient
{
    public class PlayerJoinCommand : IServerToClientCommandHandler
    {
        public ServerToClientEvent CommandType => ServerToClientEvent.PLAYER_JOIN;
        public int PacketSize => 5;

        public int PlayerId { get; private set; }

        // 🔥 Описание структуры пакета
        public static Dictionary<string, int> FieldOffsets { get; protected set; } = new()
        {
            { "PlayerId", 1 }
        };

        public PlayerJoinCommand() { }

        public PlayerJoinCommand(int playerId)
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
            UnityEngine.Debug.Log($"Игрок {PlayerId} присоединился к игре!");

            // 🔥 Здесь можно обновить список игроков в Unity
        }
    }
}