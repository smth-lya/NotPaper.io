using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using GameServer;
using GameShared.Interfaces;
using GameShared.Entity;

namespace GameShared
{
    public class PaperServer
    {
        private readonly int _port;
        public readonly int MaxPlayers;
        private readonly Socket _serverSocket;
        // Здесь будут храниться команды, которые сервер может обработать
        private readonly ServerCommandFactory _commandFactory; // Фабрика команд
        public readonly ConcurrentDictionary<int, Player> Players = new();
        public Dictionary<int, (float X, float Y)> PlayerPositions { get; private set; } = new();

        private readonly object _lock = new();

        public PaperServer(int port, int maxPlayers, IEnumerable<Func<IClientToServerCommandHandler>> commandFactories)
        {
            _port = port;
            MaxPlayers = maxPlayers;
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _commandFactory = new ServerCommandFactory(commandFactories);
        }

        public async Task Start()
        {
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
            _serverSocket.Listen(MaxPlayers);
            Console.WriteLine($"Сервер запущен на порту {_port}");
            await AcceptClients();
        }

        private async Task AcceptClients()
        {
            while (true)
            {
                Socket clientSocket = await _serverSocket.AcceptAsync();
                Console.WriteLine($"Новый игрок подключился: {clientSocket.RemoteEndPoint}");
                _ = Task.Run(() => HandleClient(clientSocket));
            }
        }

        private async Task HandleClient(Socket clientSocket)
        {
            try
            {
                List<byte> messageBuffer = new List<byte>();

                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int receivedBytes =
                        await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    if (receivedBytes == 0) break;

                    messageBuffer.AddRange(buffer[..receivedBytes]);

                    if (messageBuffer.Count < 1)
                        continue;

                    ClientToServerEvent commandType = (ClientToServerEvent)messageBuffer[0];

                    int expectedSize = _commandFactory.GetCommandSize(commandType);
                    if (messageBuffer.Count < expectedSize)
                        continue;

                    byte[] fullMessage = messageBuffer.GetRange(0, expectedSize).ToArray();
                    messageBuffer.RemoveRange(0, expectedSize);

                    IClientToServerCommandHandler? command =
                        _commandFactory.ParseCommand(fullMessage, clientSocket, this);
                    if (command != null)
                    {
                        Console.WriteLine($"Обработана команда: {command.CommandType}");
                        await command.Execute(this, clientSocket);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в HandleClient: {ex.Message}");
            }
            finally
            {
                clientSocket.Close();
            }
        }
        
        public int GeneratePlayerId()
        {
            int newId;
            do
            {
                newId = new Random().Next(1, 10000);
            } while (Players.ContainsKey(newId)); // Генерируем, пока ID уникален

            return newId;
        }

        public async Task Broadcast(byte[] data)
        {
            List<Task> sendTasks = new List<Task>();

            foreach (var player in Players.Values)
            {
                try
                {
                    sendTasks.Add(player.Socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None));
                }
                catch
                {
                    Console.WriteLine($"Ошибка отправки данных игроку {player.Id}");
                }
            }

            await Task.WhenAll(sendTasks); // Ждём завершения всех отправок
        }
        
        
        public async Task Broadcast(byte[] data, Func<Player, bool> predicate)
        {
            List<Task> sendTasks = new List<Task>();

            foreach (var player in Players.Values.Where(predicate))
            {
                try
                {
                    sendTasks.Add(player.Socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None));
                }
                catch
                {
                    Console.WriteLine($"Ошибка отправки данных игроку {player.Id}");
                }
            }

            await Task.WhenAll(sendTasks); // 🔥 Ждём завершения всех отправок
        }
    }
}