using System.Net.Sockets;
using GameShared;
using GameShared.Commands.ServerToClient;
using GameShared.Interfaces;

namespace GameShared.Commands.ClientToServer
{
    public class ExitCommand : IClientToServerCommandHandler
    {
        public ClientToServerEvent CommandType => ClientToServerEvent.EXIT;
        public int PacketSize => 5; // 1 байт - команда, 4 байта - PlayerId

        public int PlayerId { get; private set; }

        public static Dictionary<string, int> FieldOffsets { get; protected set; } = new()
        {
            { "PlayerId", 1 }
        };

        public ExitCommand() { }

        public ExitCommand(int playerId)
        {
            PlayerId = playerId;
        }

        public void ParseFromBytes(byte[] data)
        {
            PlayerId = BitConverter.ToInt32(data, FieldOffsets["PlayerId"]);
        }

        public byte[] ToBytes()
        {
            byte[] result = new byte[PacketSize];
            result[0] = (byte)CommandType;
            BitConverter.GetBytes(PlayerId).CopyTo(result, FieldOffsets["PlayerId"]);
            return result;
        }
        
        public async Task Execute(PaperServer server, Socket clientSocket)
        {
            Console.WriteLine($"Игрок {PlayerId} выходит из игры...");

            if (server.Players.TryRemove(PlayerId, out _))
            {
                Console.WriteLine($"Игрок {PlayerId} удалён.");

                // 🔥 Уведомляем всех игроков о выходе
                var playerExitCommand = new PlayerExitCommand(PlayerId);
                await server.Broadcast(playerExitCommand.ToBytes());
            }
            else
            {
                Console.WriteLine($"Ошибка: Игрок {PlayerId} не найден.");
            }

            clientSocket.Close();
        }
    }
}