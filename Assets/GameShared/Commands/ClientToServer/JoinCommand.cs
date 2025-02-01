using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;
using GameShared.Commands.ServerToClient;
using GameShared.Entity;
using Random = System.Random;

namespace GameShared.Commands.ClientToServer
{
    /// <summary>
    /// Команда для присоединения игрока к серверу.
    /// </summary>
    public sealed class JoinCommand : ClientToServerCommand
    {
        private readonly Random _random = new();

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

            Debug.Log("Игрок хочет присоединиться...");

            if (server.Players.Count >= server.MaxPlayers)
            {
                Debug.Log("Сервер заполнен. Отказано.");
                return;
            }

            int newPlayerId = server.GeneratePlayerId();
            (Vector3 startPosition, Vector3 startDirection) = GenerateRandomSpawnPoint();

            Debug.Log(newPlayerId + " ДОБАВЛЕН");

            var player = new PaperPlayer(newPlayerId, clientSocket)
            {
                Position = startPosition,
                Direction = startDirection
            };

            server.Players.TryAdd(newPlayerId, player);
            server.PlayerPositions[newPlayerId] = (startPosition, startDirection);

            Debug.Log($"Игрок {newPlayerId} подключен на ({startPosition}) с направлением {startDirection}");

            await SendWelcomePacket(clientSocket, newPlayerId, startPosition, startDirection);
            await server.BroadcastAsync(new RequestPositionsCommand().ToBytes());
        }

        /// <summary>
        /// Генерирует случайную позицию и направление для нового игрока.
        /// </summary>
        private (Vector3 position, Vector3 direction) GenerateRandomSpawnPoint()
        {
            Vector3 position = new Vector3()
            {
                x = _random.Next(-10, 10),
                z = _random.Next(-10, 10),
            };

            Vector3 direction = new Vector3()
            {
                x = (float)_random.NextDouble() * 2 - 1, // Так сделано для обеспечения захвата отрицательных значений [-1; 1]
                z = (float)_random.NextDouble() * 2 - 1,
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
                Debug.Log($"Ошибка отправки WELCOME-пакета: {ex.Message}");
            }
        }
    }
}
