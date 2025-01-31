using System;
using System.Collections.Generic;
using System.Numerics;
using GameShared.Interfaces;

namespace GameShared.Commands.ServerToClient
{
    public class GameStateCommand : IServerToClientCommandHandler
    {
        public ServerToClientEvent CommandType => ServerToClientEvent.GAME_STATE;
        public int PacketSize => 1 + Players.Count * 20; // 1 байт - команда, 4 - PlayerId, 4 - X, 4 - Z, 4 - DirX, 4 - DirZ

        public List<(int PlayerId, float X, float Z, Vector2 Direction)> Players { get; private set; } = new();

        public static Dictionary<string, int> FieldOffsets { get; protected set; } = new()
        {
            { "PlayerId", 1 },
            { "X", 5 },
            { "Z", 9 },
            { "DirX", 13 },
            { "DirZ", 17 }
        };

        public GameStateCommand() { }

        public GameStateCommand(List<(int PlayerId, float X, float Z, Vector2 Direction)> players)
        {
            Players = players;
        }

        public void ParseFromBytes(byte[] data)
        {
            Players.Clear();
            int count = (data.Length - 1) / 20; // 1 байт - команда, 20 байт на каждого игрока
            for (int i = 0; i < count; i++)
            {
                int playerId = BitConverter.ToInt32(data, 1 + i * 20);
                float x = BitConverter.ToSingle(data, 5 + i * 20);
                float z = BitConverter.ToSingle(data, 9 + i * 20);
                float dirX = BitConverter.ToSingle(data, 13 + i * 20);
                float dirZ = BitConverter.ToSingle(data, 17 + i * 20);
                Players.Add((playerId, x, z, new Vector2(dirX, dirZ)));
            }
        }

        public byte[] ToBytes()
        {
            byte[] result = new byte[PacketSize];
            result[0] = (byte)CommandType;

            for (int i = 0; i < Players.Count; i++)
            {
                BitConverter.GetBytes(Players[i].PlayerId).CopyTo(result, 1 + i * 20);
                BitConverter.GetBytes(Players[i].X).CopyTo(result, 5 + i * 20);
                BitConverter.GetBytes(Players[i].Z).CopyTo(result, 9 + i * 20);
                BitConverter.GetBytes(Players[i].Direction.X).CopyTo(result, 13 + i * 20);
                BitConverter.GetBytes(Players[i].Direction.Y).CopyTo(result, 17 + i * 20);
            }

            return result;
        }

        public async Task Execute(PaperClient client)
        {
            Console.WriteLine("[Client] Получен `GameStateCommand`, обновляем позиции игроков...");
        }
    }
}
