using System;
using System.Collections.Generic;
using System.Numerics;
using GameShared.Entity;
using GameShared.Interfaces;

namespace GameShared.Commands.ServerToClient
{
    public class WelcomeCommand : IServerToClientCommandHandler
    {
        public ServerToClientEvent CommandType => ServerToClientEvent.WELCOME;
        public int PacketSize => 17; // 1 байт - команда, 4 байта - PlayerId, 4 - X, 4 - Z, 4 - DirX, 4 - DirZ

        public int PlayerId { get; private set; }
        public float X { get; private set; }
        public float Z { get; private set; }
        public Vector2 Direction { get; private set; }

        public static Dictionary<string, int> FieldOffsets { get; protected set; } = new()
        {
            { "PlayerId", 1 },
            { "X", 5 },
            { "Z", 9 },
            { "DirX", 13 },
            { "DirZ", 17 }
        };

        public WelcomeCommand() { }

        public WelcomeCommand(int playerId, float x, float z, Vector2 direction)
        {
            PlayerId = playerId;
            X = x;
            Z = z;
            Direction = direction;
        }

        public void ParseFromBytes(byte[] data)
        {
            PlayerId = BitConverter.ToInt32(data, FieldOffsets["PlayerId"]);
            X = BitConverter.ToSingle(data, FieldOffsets["X"]);
            Z = BitConverter.ToSingle(data, FieldOffsets["Z"]);
            float dirX = BitConverter.ToSingle(data, FieldOffsets["DirX"]);
            float dirZ = BitConverter.ToSingle(data, FieldOffsets["DirZ"]);
            Direction = new Vector2(dirX, dirZ);
        }

        public byte[] ToBytes()
        {
            byte[] result = new byte[PacketSize];
            result[0] = (byte)CommandType;
            BitConverter.GetBytes(PlayerId).CopyTo(result, FieldOffsets["PlayerId"]);
            BitConverter.GetBytes(X).CopyTo(result, FieldOffsets["X"]);
            BitConverter.GetBytes(Z).CopyTo(result, FieldOffsets["Z"]);
            BitConverter.GetBytes(Direction.X).CopyTo(result, FieldOffsets["DirX"]);
            BitConverter.GetBytes(Direction.Y).CopyTo(result, FieldOffsets["DirZ"]);
            return result;
        }

        public async Task Execute(PaperClient client)
        {
            Console.WriteLine($"[Client] WELCOME. PlayerId={PlayerId}, X={X}, Z={Z}, Direction={Direction}");

            // Устанавливаем начальные параметры игрока
            client.PlayerId = PlayerId;
            client.PlayerData = new PlayerData { X = X, Z = Z, Direction = Direction };
        }
    }
}
