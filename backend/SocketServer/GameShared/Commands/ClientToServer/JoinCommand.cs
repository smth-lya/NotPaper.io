using System.Net.Sockets;
using GameShared.Commands.ServerToClient;
using GameShared.Entity;
using GameShared.Interfaces;

namespace GameShared.Commands.ClientToServer
{
    public class JoinCommand : IClientToServerCommandHandler
    {
        public ClientToServerEvent CommandType => ClientToServerEvent.JOIN;
        public int PacketSize => 1; // 1 байт - команда (без PlayerId)

        public static Dictionary<string, int> FieldOffsets { get; protected set; } = new();

        public void ParseFromBytes(byte[] data) { }

        public byte[] ToBytes()
        {
            byte[] result = new byte[PacketSize];
            result[0] = (byte)CommandType;
            return result;
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

            // Добавляем игрока
            var player = new Player(newPlayerId, clientSocket);
            server.Players.TryAdd(newPlayerId, player);

            Console.WriteLine($"Игрок {newPlayerId} подключен!");

            // 🔥 Отправляем `WELCOME` с `PlayerId`
            var welcomeCommand = new WelcomeCommand(newPlayerId);
            await clientSocket.SendAsync(new ArraySegment<byte>(welcomeCommand.ToBytes()), SocketFlags.None);

            // 🔥 Уведомляем всех игроков о новом подключении
            var playerJoinCommand = new PlayerJoinCommand(newPlayerId);
            await server.Broadcast(playerJoinCommand.ToBytes());
        }
    }
}