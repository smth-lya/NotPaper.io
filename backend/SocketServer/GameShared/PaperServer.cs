using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GameServer;
using GameShared.Interfaces;
using GameShared.Entity;

namespace GameShared
{
    public class PaperServer
    {
        private readonly int _port;
        private readonly int _maxPlayers;
        private readonly Socket _serverSocket;
        private readonly ServerCommandFactory _commandFactory; // 🔥 Фабрика команд
        private readonly Dictionary<int, Player> _players = new();
        private readonly object _lock = new();

        public PaperServer(int port, int maxPlayers, IEnumerable<Func<IClientToServerCommandHandler>> commandFactories)
        {
            _port = port;
            _maxPlayers = maxPlayers;
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _commandFactory = new ServerCommandFactory(commandFactories);
        }

        public async Task Start()
        {
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
            _serverSocket.Listen(_maxPlayers);
            Console.WriteLine($"Сервер запущен на порту {_port}");
            await AcceptClients();
        }

        private async Task AcceptClients()
        {
            while (true)
            {
                Socket clientSocket = await _serverSocket.AcceptAsync();
                Console.WriteLine($"Новый игрок подключился: {clientSocket.RemoteEndPoint}");
                _ = Task.Run(() => { HandleClient(clientSocket); });
            }
        }

        private void HandleClient(Socket clientSocket)
        {
            try
            {
                List<byte> messageBuffer = new List<byte>();

                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int receivedBytes = clientSocket.Receive(buffer);
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
                        command.Execute(this); // 🔥 Теперь передаём `Socket`
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

        public void Broadcast(byte[] data)
        {
            lock (_lock)
            {
                foreach (var player in _players.Values)
                {
                    try
                    {
                        player.Socket.Send(data);
                    }
                    catch
                    {
                        Console.WriteLine($"Ошибка отправки данных игроку {player.Id}");
                    }
                }
            }
        }
    }
}