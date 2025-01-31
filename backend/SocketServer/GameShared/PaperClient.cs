using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using GameShared.Commands.ClientToServer;
using GameShared.Entity;
using GameShared.Factories;

namespace GameShared
{
    public class PaperClient
    {
        private readonly INetworkClient _networkClient;
        private readonly ClientCommandFactory _commandFactory;
        private bool _isRunning;

        public BasePlayer Context { get; set; } = null!;

        // Событие для Unity
        public event Action<ServerToClientEvent, ServerToClientCommand>? OnCommandReceived;

        public PaperClient(IPAddress serverIp, int serverPort, IEnumerable<Func<ServerToClientCommand>> commandFactories)
        {
            _networkClient = new NetworkClient(serverIp, serverPort);
            _commandFactory = new ClientCommandFactory(commandFactories);
        }

        public async Task StartAsync()
        {
            try
            {
                await _networkClient.ConnectAsync();
                Console.WriteLine($"[Client] Подключен к серверу {_networkClient.ServerIp}:{_networkClient.ServerPort}");

                await SendJoinRequestAsync();

                _isRunning = true;
                await ListenToServerAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при подключении: {ex.Message}");
            }
        }

        private async Task SendJoinRequestAsync()
        {
            Console.WriteLine("[Client] Отправка запроса на вход в лобби...");
            var joinCommand = new JoinCommand();
            byte[] joinPacket = joinCommand.ToBytes();

            await _networkClient.SendAsync(joinPacket);
        }

        private async Task ListenToServerAsync()
        {
            try
            {
                while (_isRunning)
                {
                    byte[] buffer = await _networkClient.ReceiveAsync();
                    if (buffer.Length > 0)
                    {
                        _ = Task.Run(() => HandleServerResponseAsync(buffer));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных: {ex.Message}");
            }
        }

        private async Task HandleServerResponseAsync(byte[] data)
        {
            ServerToClientEvent commandType = (ServerToClientEvent)data[0];

            var command = _commandFactory.ParseCommand(data, this);
            if (command != null)
            {
                await command.ExecuteAsync(this);

                Console.WriteLine($"Получено сообщение: {command.CommandType}");

                // Вызываем событие для Unity
                OnCommandReceived?.Invoke(commandType, command);
            }
        }

        public async Task SendCommandAsync(ClientToServerCommand command)
        {
            byte[] packet = command.ToBytes();
            await _networkClient.SendAsync(packet);
        }

        public async Task ChangeDirectionAsync(Vector3 direction)
        {
            Console.WriteLine($"[Client] Игрок {Context.Id} сменил направление на {direction}");
            var moveCommand = new MoveCommand(Context.Id, direction);
            byte[] movePacket = moveCommand.ToBytes();

            await _networkClient.SendAsync(movePacket);
        }

        public async Task ExitAsync()
        {
            Console.WriteLine($"[Client] Игрок {Context.Id} отправляет запрос на выход...");

            var exitCommand = new ExitCommand(Context.Id);
            await SendCommandAsync(exitCommand);

            _isRunning = false;
            _networkClient.Close();
        }
    }

}
