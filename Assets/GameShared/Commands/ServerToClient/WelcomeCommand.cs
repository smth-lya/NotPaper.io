using GameShared;
using GameShared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameShared.Commands.ServerToClient
{
    public class WelcomeCommand : IServerToClientCommandHandler
    {
        public ServerToClientEvent CommandType => ServerToClientEvent.WELCOME;
        public int PacketSize => 5; // 1 байт - команда, 4 байта - PlayerId

        public int PlayerId { get; private set; }

        public static Dictionary<string, int> FieldOffsets { get; protected set; } = new()
        {
            { "PlayerId", 1 }
        };

        public WelcomeCommand() { }

        public WelcomeCommand(int playerId)
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
            UnityEngine.Debug.Log($"[Client] Получен `WELCOME`, мой PlayerId = {PlayerId}");
            client.PlayerId = PlayerId; // Теперь клиент сохраняет PlayerId
        }
    }
}