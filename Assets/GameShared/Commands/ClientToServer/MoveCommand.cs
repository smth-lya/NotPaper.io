using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using GameShared.Commands.ServerToClient;
using GameShared.Interfaces;

namespace GameShared.Commands.ClientToServer
{
    public class MoveCommand : IClientToServerCommandHandler
    {
        public ClientToServerEvent CommandType => ClientToServerEvent.MOVE;
        public int PacketSize => 6; // 1 байт - команда, 4 байта - PlayerId, 1 байт - направление

        public int PlayerId { get; private set; }
        public int Direction { get; private set; } // 0 = NONE, 1 = UP, 2 = DOWN, 3 = LEFT, 4 = RIGHT

        public static Dictionary<string, int> FieldOffsets { get; protected set; } = new()
    {
        { "PlayerId", 1 }, // ID игрока с 1-го байта
        { "Direction", 5 }  // Направление с 5-го байта
    };

        public MoveCommand() { }

        public MoveCommand(int playerId, int direction)
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

        public async Task Execute(PaperServer server, Socket clientSocket)
        {
            UnityEngine.Debug.Log($"Игрок {PlayerId} сменил направление на {Direction}");

            if (server.Players.TryGetValue(PlayerId, out var player))
            {
                player.CurrentDirection = Direction;
            }

            // 🔥 Создаём команду `PLAYER_MOVE`
            byte[] response = new PlayerMoveCommand(PlayerId, Direction).ToBytes();
            await server.Broadcast(response);
        }
    }
}