using System;
using System.Net.Sockets;
using System.Numerics;
using GameShared.Commands.ServerToClient;
using GameShared.Entity;
using GameShared.Interfaces;

namespace GameShared.Commands.ClientToServer
{
    public class JoinCommand : IClientToServerCommandHandler
    {
        public ClientToServerEvent CommandType => ClientToServerEvent.JOIN;
        public int PacketSize => 1;

        public static Dictionary<string, int> FieldOffsets { get; protected set; } = new();

        public void ParseFromBytes(byte[] data) { }

        public byte[] ToBytes()
        {
            return new byte[] { (byte)CommandType };
        }

        public async Task Execute(PaperServer server, Socket clientSocket)
        {
            Console.WriteLine("Игрок хочет присоединиться...");

            if (server.Players.Count >= server.MaxPlayers)
            {
                Console.WriteLine("Сервер заполнен. Отказано.");
                return;
            }

            // 🔥 Генерируем уникальный `PlayerId`
            int newPlayerId = server.GeneratePlayerId();

            // 🔥 Генерируем случайную позицию и направление
            Random rnd = new Random();
            float startX = rnd.Next(0, 100);
            float startZ = rnd.Next(0, 100);

            Vector2[] possibleDirections = { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
            Vector2 startDirection = possibleDirections[rnd.Next(possibleDirections.Length)];

            // Добавляем игрока
            var player = new Player(newPlayerId, clientSocket)
            {
                X = startX,
                Z = startZ,
                Direction = startDirection
            };
            server.Players.TryAdd(newPlayerId, player);
            server.PlayerPositions[newPlayerId] = (startX, startZ, startDirection);

            Console.WriteLine($"Игрок {newPlayerId} подключен на ({startX}, {startZ}) с направлением {startDirection}");

            // 🔥 Отправляем `WELCOME` с `PlayerId`, X, Z и направлением
            var welcomeCommand = new WelcomeCommand(newPlayerId, startX, startZ, startDirection);
            await clientSocket.SendAsync(new ArraySegment<byte>(welcomeCommand.ToBytes()), SocketFlags.None);

            // 🔥 Запрашиваем у всех игроков их позиции
            var requestPositionsCommand = new RequestPositionsCommand();
            await server.Broadcast(requestPositionsCommand.ToBytes());
        }
    }
}
