using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using GameServer;
using GameShared.Interfaces;
using GameShared.Entity;
using System.Linq;
using UnityEngine;
using System.Numerics;

namespace GameShared
{
    public class PaperServer
    {
        private readonly int _port;
        public readonly int MaxPlayers;
        private readonly Socket _serverSocket;
        // Здесь будут храниться команды, которые сервер может обработать
        private readonly ServerCommandFactory _commandFactory; // Фабрика команд
        public readonly ConcurrentDictionary<int, PlayerNet> Players = new();
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
            UnityEngine.Debug.Log($"Сервер запущен на порту {_port}");
            await AcceptClients();
        }

        private async Task AcceptClients()
        {
            while (true)
            {
                Socket clientSocket = await _serverSocket.AcceptAsync();
                UnityEngine.Debug.Log($"Новый игрок подключился: {clientSocket.RemoteEndPoint}");
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
                    //if (command != null)
                    //{
                    //    UnityEngine.Debug.Log($"Обработана команда: {command.CommandType}");
                    //    await command.Execute(this, clientSocket);
                    //}
                    await command?.Execute(this, clientSocket)!;
                    UnityEngine.Debug.Log($"Обработана команда: {command.CommandType}");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"Ошибка в HandleClient: {ex.Message}");
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
                newId = new System.Random().Next(1, 10000);
            } while (Players.ContainsKey(newId)); // Генерируем, пока ID уникален

            return newId;
        }

        public async Task Broadcast(byte[] data)
        {
            List<Task> sendTasks = new List<Task>();

            foreach (var PlayerNet in Players.Values)
            {
                try
                {
                    sendTasks.Add(PlayerNet.Socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None));
                }
                catch
                {
                    UnityEngine.Debug.Log($"Ошибка отправки данных игроку {PlayerNet.Id}");
                }
            }

            await Task.WhenAll(sendTasks); // Ждём завершения всех отправок
        }
        
        
        public async Task Broadcast(byte[] data, Func<PlayerNet, bool> predicate)
        {
            List<Task> sendTasks = new List<Task>();

            foreach (var Player in Players.Values.Where(predicate))
            {
                try
                {
                    sendTasks.Add(Player.Socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None));
                }
                catch
                {
                    UnityEngine.Debug.Log($"Ошибка отправки данных игроку {Player.Id}");
                }
            }

            await Task.WhenAll(sendTasks); // 🔥 Ждём завершения всех отправок
        }
    }
}