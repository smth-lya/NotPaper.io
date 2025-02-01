using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;
using GameServer;
using GameShared.Entity;
using Random = System.Random;

namespace GameShared
{
    /// <summary>
    /// Серверная часть игры, обрабатывающая подключения игроков и команды.
    /// </summary>
    public class PaperServer
    {
        private readonly int _port;
        public int MaxPlayers { get; }
        private readonly Socket _serverSocket;
        private readonly ServerCommandFactory _commandFactory;

        private readonly Random _random = new();

        public ConcurrentDictionary<int, PaperPlayer> Players { get; } = new();
        public ConcurrentDictionary<int, (Vector3 Position, Vector3 Direction)> PlayerPositions { get; } = new();

        private readonly object _lock = new();

        public PaperServer(int port, int maxPlayers, IEnumerable<Func<ClientToServerCommand>> commandFactories)
        {
            _port = port;
            MaxPlayers = maxPlayers;
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _commandFactory = new ServerCommandFactory(commandFactories);
        }

        /// <summary>
        /// Запускает сервер и начинает прием клиентов.
        /// </summary>
        public async Task StartAsync()
        {
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
            _serverSocket.Listen(MaxPlayers);
            Debug.Log($"Сервер запущен на порту {_port}");

            while (true)
            {
                Socket clientSocket = await _serverSocket.AcceptAsync();
                Debug.Log($"Новый игрок подключился: {clientSocket.RemoteEndPoint}");
                _ = Task.Run(() => HandleClientAsync(clientSocket));
            }
        }

        /// <summary>
        /// Обрабатывает подключенного клиента и его команды.
        /// </summary>
        private async Task HandleClientAsync(Socket clientSocket)
        {
            try
            {
                var messageBuffer = new List<byte>();

                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int receivedBytes = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                    if (receivedBytes == 0)
                        break;

                    messageBuffer.AddRange(buffer[..receivedBytes]);

                    if (messageBuffer.Count < 1)
                        continue;

                    var commandType = (ClientToServerEvent)messageBuffer[0];
                    int expectedSize = _commandFactory.GetCommandSize(commandType);

                    if (messageBuffer.Count < expectedSize)
                        continue;

                    byte[] fullMessage = messageBuffer.Take(expectedSize).ToArray();
                    messageBuffer.RemoveRange(0, expectedSize);

                    var command = _commandFactory.ParseCommand(fullMessage, clientSocket, this);

                    if (command != null)
                    {
                        Debug.Log($"Обработана команда: {command.CommandType}");
                        await command.ExecuteAsync(this, clientSocket);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Ошибка в HandleClientAsync: {ex.Message}");
            }
            finally
            {
                clientSocket.Close();
            }
        }

        /// <summary>
        /// Генерирует уникальный ID для нового игрока.
        /// </summary>
        public int GeneratePlayerId()
        {
            int newId;
            lock (_lock)
            {
                do
                {
                    newId = _random.Next(1, 10000);
                } while (Players.ContainsKey(newId));
            }
            return newId;
        }

        /// <summary>
        /// Рассылает данные всем игрокам.
        /// </summary>
        public async Task BroadcastAsync(byte[] data)
            => await BroadcastAsync(data, _ => true);

        /// <summary>
        /// Рассылает данные игрокам, соответствующим предикату.
        /// </summary>
        public async Task BroadcastAsync(byte[] data, Func<PaperPlayer, bool> predicate)
        {
            var sendTasks = Players.Values
                .Where(predicate)
                .Select(player => SendDataAsync(player, data))
                .ToList();

            await Task.WhenAll(sendTasks);
        }

        /// <summary>
        /// Отправляет данные конкретному игроку с обработкой ошибок.
        /// </summary>
        private static async Task SendDataAsync(PaperPlayer player, byte[] data)
        {
            try
            {
                await player.Socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
            }
            catch (SocketException)
            {
                Debug.Log($"Ошибка отправки данных игроку {player.Id}");
            }
        }
    }
}
