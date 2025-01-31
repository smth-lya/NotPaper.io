using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;
using GameShared.Commands.ServerToClient;
using GameShared.Entity;

namespace GameShared.Commands.ClientToServer
{
    /// <summary>
    /// Команда для присоединения игрока к серверу.
    /// </summary>
    public sealed class JoinCommand : ClientToServerCommand
    {
        public override ClientToServerEvent CommandType => ClientToServerEvent.JOIN;
        public override int PacketSize => sizeof(byte); // 1 байт - команда.

        public override void ParseFromBytes(byte[] data) { }
        public override byte[] ToBytes() 
            => new byte[] { (byte)CommandType };

        /// <summary>
        /// Выполняет команду присоединения игрока к серверу.
        /// </summary>
        /// <param name="server">Экземпляр сервера.</param>
        /// <param name="clientSocket">Сокет клиента.</param>
        /// <returns>Асинхронная задача.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если server или clientSocket равны null.</exception>
        public override async Task ExecuteAsync(PaperServer server, Socket clientSocket)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            if (clientSocket == null) throw new ArgumentNullException(nameof(clientSocket));

            Console.WriteLine("Игрок хочет присоединиться...");

            if (server.Players.Count >= server.MaxPlayers)
            {
                Console.WriteLine("Сервер заполнен. Отказано.");
                return;
            }

            int newPlayerId = server.GeneratePlayerId();
            (Vector3 startPosition, Vector3 startDirection) = GenerateRandomSpawnPoint();

            var player = new PaperPlayer(newPlayerId, clientSocket)
            {
                Position = startPosition,
                Direction = startDirection
            };

            server.Players.TryAdd(newPlayerId, player);
            server.PlayerPositions[newPlayerId] = (startPosition, startDirection);

            Console.WriteLine($"Игрок {newPlayerId} подключен на ({startPosition}) с направлением {startDirection}");

            await SendWelcomePacket(clientSocket, newPlayerId, startPosition, startDirection);
            await server.BroadcastAsync(new RequestPositionsCommand().ToBytes());
        }

        /// <summary>
        /// Генерирует случайную позицию и направление для нового игрока.
        /// </summary>
        private static (Vector3 position, Vector3 direction) GenerateRandomSpawnPoint()
        {
            Vector3 position = new Vector3()
            {
                X = Random.Shared.Next(-20, 20),
                Z = Random.Shared.Next(-20, 20),
            };

            Vector3 direction = new Vector3()
            {
                X = Random.Shared.NextSingle() * 2 - 1, // Так сделано для обеспечения захвата отрицательных значений [-1; 1]
                Z = Random.Shared.NextSingle() * 2 - 1,
            };

            return (position, direction);
        }

        /// <summary>
        /// Отправляет игроку приветственный пакет с информацией о его местоположении.
        /// </summary>
        private static async Task SendWelcomePacket(Socket clientSocket, int playerId, Vector3 position, Vector3 direction)
        {
            var welcomeCommand = new WelcomeCommand(playerId, position, direction);
            var welcomeBytes = welcomeCommand.ToBytes();

            try
            {
                await clientSocket.SendAsync(new ArraySegment<byte>(welcomeBytes), SocketFlags.None);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка отправки WELCOME-пакета: {ex.Message}");
            }
        }
    }
}
