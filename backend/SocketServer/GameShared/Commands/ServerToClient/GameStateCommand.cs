using GameShared;
using GameShared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace GameShared.Commands.ServerToClient
{
    public class GameStateCommand : IServerToClientCommandHandler
    {
        public ServerToClientEvent CommandType => ServerToClientEvent.GAME_STATE;
        public int PacketSize => 1 + Players.Count * 12; // 1 байт - команда, 4 байта - PlayerId, 4 байта - X, 4 байта - Y

        public List<(int PlayerId, float X, float Y)> Players { get; private set; } = new();

        public static Dictionary<string, int> FieldOffsets { get; protected set; } = new()
        {
            { "PlayerId", 1 },
            { "X", 5 },
            { "Y", 9 }
        };

        public GameStateCommand() { }

        public GameStateCommand(List<(int PlayerId, float X, float Y)> players)
        {
            Players = players;
        }

        public void ParseFromBytes(byte[] data)
        {
            Players.Clear();
            int count = (data.Length - 1) / 12; // 1 байт - команда, 12 байт на каждого игрока
            for (int i = 0; i < count; i++)
            {
                int playerId = BitConverter.ToInt32(data, 1 + i * 12);
                float x = BitConverter.ToSingle(data, 5 + i * 12);
                float y = BitConverter.ToSingle(data, 9 + i * 12);
                Players.Add((playerId, x, y));
            }
        }

        public byte[] ToBytes()
        {
            byte[] result = new byte[PacketSize];
            result[0] = (byte)CommandType;

            for (int i = 0; i < Players.Count; i++)
            {
                BitConverter.GetBytes(Players[i].PlayerId).CopyTo(result, 1 + i * 12);
                BitConverter.GetBytes(Players[i].X).CopyTo(result, 5 + i * 12);
                BitConverter.GetBytes(Players[i].Y).CopyTo(result, 9 + i * 12);
            }

            return result;
        }

        public async Task Execute(PaperServer server, Socket clientSocket)
        {
            Console.WriteLine($"Игрок {PlayerId} сообщил свою позицию: X={X}, Y={Y}");

            server.PlayerPositions[PlayerId] = (X, Y);

            // 🔥 Проверяем, прислали ли все игроки свои позиции
            if (server.PlayerPositions.Count == server.Players.Count)
            {
                Console.WriteLine("[Server] Все игроки отправили позиции, рассылаем `GameStateCommand`.");
                var gameStateCommand = new GameStateCommand(
                    server.Players.Values.Select(p => (p.Id, server.PlayerPositions[p.Id].X, server.PlayerPositions[p.Id].Y)).ToList()
                );
                await server.Broadcast(gameStateCommand.ToBytes());
            }
        }
    }
}
