using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;
using GameShared.Commands.ServerToClient;
using GameShared.Entity;

namespace GameShared.Commands.ClientToServer
{
    public sealed class SendPositionCommand : ClientToServerCommand
    {
        private static readonly Dictionary<string, int> _fieldOffsets = new()
        {
            { "PlayerId", 1 },
            { "X", 5 },
            { "Z", 9 },
            { "DirX", 13 },
            { "DirZ", 17 }
        };

        public override ClientToServerEvent CommandType => ClientToServerEvent.SEND_POSITION;
        
        // 1 байт - команда, 4 - PlayerId, 4 - X, 4 - Z, 4 - DirX, 4 - DirZ
        public override int PacketSize => sizeof(byte) + sizeof(int) + sizeof(float) * 4; 

        public int PlayerId { get; private set; }
     
        public Vector3 Position { get; private set; }
        public Vector3 Direction { get; private set; }

        public SendPositionCommand() { }

        public SendPositionCommand(BasePlayer context)
        {
            PlayerId = context.Id;
            Position = context.Position;
            Direction = context.Direction;
        }

        public SendPositionCommand(int playerId, Vector3 position, Vector3 direction)
        {
            PlayerId = playerId;
            Position = position;
            Direction = direction;
        }

        public override void ParseFromBytes(byte[] data)
        {
            PlayerId = BitConverter.ToInt32(data, _fieldOffsets["PlayerId"]);

            Position = new Vector3()
            {
                x = BitConverter.ToSingle(data, _fieldOffsets["X"]),
                z = BitConverter.ToSingle(data, _fieldOffsets["Z"])
            };

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

            BitConverter.GetBytes(Position.x).CopyTo(result, _fieldOffsets["X"]);
            BitConverter.GetBytes(Position.z).CopyTo(result, _fieldOffsets["Z"]);

            BitConverter.GetBytes(Direction.x).CopyTo(result, _fieldOffsets["DirX"]);
            BitConverter.GetBytes(Direction.z).CopyTo(result, _fieldOffsets["DirZ"]);

            return result;
        }

        public override async Task ExecuteAsync(PaperServer server, Socket clientSocket)
        {
            Debug.Log($"Игрок {PlayerId} сообщил свою позицию: X={Position.x}, Y = {Position.y}, Z={Position.z}, Direction={Direction}");

            server.PlayerPositions[PlayerId] = (Position, Direction);

            // 🔥 Проверяем, прислали ли все игроки свои позиции
            if (server.PlayerPositions.Count == server.Players.Count)
            {
                Debug.Log("[Server] Все игроки отправили позиции, рассылаем `GameStateCommand`.");
                var gameStateCommand = new GameStateCommand(
                    server.Players.Values.Select(p => new BasePlayer
                    {
                        Id = p.Id,
                        Position = server.PlayerPositions[p.Id].Position,
                        Direction = server.PlayerPositions[p.Id].Direction
                    }).ToList()
                );
                await server.BroadcastAsync(gameStateCommand.ToBytes());
            }
        }
    }
}
