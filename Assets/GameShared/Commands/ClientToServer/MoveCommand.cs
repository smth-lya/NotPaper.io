using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;
using GameShared.Commands.ServerToClient;

namespace GameShared.Commands.ClientToServer
{
    public sealed class MoveCommand : ClientToServerCommand
    {
        private static readonly Dictionary<string, int> _fieldOffsets = new()
        {
            { "PlayerId", 1 },
            { "DirX", 5 },
            { "DirZ", 9 }
        };

        public override ClientToServerEvent CommandType => ClientToServerEvent.MOVE;
        public override int PacketSize => sizeof(byte) + sizeof(int) + sizeof(float) * 2; // 1 байт - команда, 4 байта - PlayerId, 1 байт - направление

        public int PlayerId { get; private set; }
        public Vector3 Direction { get; private set; } 

        public MoveCommand() { }

        public MoveCommand(int playerId, Vector3 direction)
        {
            PlayerId = playerId;
            Direction = direction;
        }

        public override void ParseFromBytes(byte[] data)
        {
            PlayerId = BitConverter.ToInt32(data, _fieldOffsets["PlayerId"]);

            Direction = new Vector3()
            {
                x = BitConverter.ToSingle(data, _fieldOffsets["DirX"]),
                z = BitConverter.ToSingle(data, _fieldOffsets["DirZ"])
            };
        }

        public override byte[] ToBytes()
        {
            byte[] result = new byte[PacketSize];
            result[0] = (byte)CommandType; 

            BitConverter.GetBytes(PlayerId).CopyTo(result, _fieldOffsets["PlayerId"]);
            BitConverter.GetBytes(Direction.x).CopyTo(result, _fieldOffsets["DirX"]);
            BitConverter.GetBytes(Direction.z).CopyTo(result, _fieldOffsets["DirZ"]);

            return result;
        }

        public override async Task ExecuteAsync(PaperServer server, Socket clientSocket)
        {
            Debug.LogWarning($"Игрок {PlayerId} сменил направление на {Direction}");

            if (server.Players.TryGetValue(PlayerId, out var player))
            {
                player.Direction = Direction;
            }

            byte[] response = new PlayerMoveCommand(PlayerId, Direction).ToBytes();
            await server.BroadcastAsync(response);
        }
    }
}