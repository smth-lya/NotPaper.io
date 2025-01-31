using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameShared.Commands.ClientToServer;
using GameShared.Factories;
using GameShared.Interfaces;

namespace GameShared
{
    public class PaperClient
    {
        private readonly IPAddress _serverIp;
        private readonly int _serverPort;
        private readonly Socket _socket;
        private readonly EndPoint _serverEP;
        // Здесь будут храниться команды, которые клиент может обработать
        private readonly ClientCommandFactory _commandFactory;
        private bool _isRunning;

        // Событие для Unity
        public event Action<ServerToClientEvent, IServerToClientCommandHandler>? OnCommandReceived;
        public int PlayerId { get;  set; } // Теперь у клиента есть `PlayerId`

        public PaperClient(IPAddress serverIp, int serverPort, IEnumerable<Func<IServerToClientCommandHandler>> commandFactories)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverEP = new IPEndPoint(_serverIp, _serverPort);
            _commandFactory = new ClientCommandFactory(commandFactories);
        }

        public async Task Start()
        {
            try
            {
                await _socket.ConnectAsync(_serverEP);
                Console.WriteLine($"[Client] Подключен к серверу {_serverIp}:{_serverPort}");

                await SendJoinRequest();

                _isRunning = true;
                await ListenToServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при подключении: {ex.Message}");
            }
        }

        private async Task SendJoinRequest()
        {
            Console.WriteLine("[Client] Отправка запроса на вход в лобби...");

            // Создаём бинарную команду `JoinCommand`
            var joinCommand = new JoinCommand();
            byte[] joinPacket = joinCommand.ToBytes();

            await _socket.SendAsync(new ArraySegment<byte>(joinPacket), SocketFlags.None);
        }

        private async Task ListenToServer()
        {
            try
            {
                while (_isRunning)
                {
                    byte[] buffer = new byte[1024];
                    int receivedBytes = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    if (receivedBytes > 0)
                    {
                        HandleServerResponse(buffer, receivedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных: {ex.Message}");
            }
        }

        private void HandleServerResponse(byte[] data, int length)
        {
            ServerToClientEvent commandType = (ServerToClientEvent)data[0];

            IServerToClientCommandHandler? command = _commandFactory.ParseCommand(data, this);
            command?.Execute(this);
            if (command != null)
            {
                Console.WriteLine($"Получено сообщение: {command.CommandType}");

                // Вызываем событие для Unity
                OnCommandReceived?.Invoke(commandType, command);
            }
        }
        
        public async Task ChangeDirection(int direction)
        {
            Console.WriteLine($"[Client] Игрок {PlayerId} сменил направление на {direction}");

            var moveCommand = new MoveCommand(PlayerId, direction);
            byte[] movePacket = moveCommand.ToBytes();

            await _socket.SendAsync(new ArraySegment<byte>(movePacket), SocketFlags.None);
        }

        public async Task Exit()
        {
            Console.WriteLine("[Client] Отправка запроса на выход...");
            byte[] exitPacket = Encoding.UTF8.GetBytes("EXIT|Player1");
            await _socket.SendAsync(new ArraySegment<byte>(exitPacket), SocketFlags.None);
            _isRunning = false;
            _socket.Close();
        }
    }
}