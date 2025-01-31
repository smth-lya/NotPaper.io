using GameShared.Commands.ClientToServer;
using GameShared.Interfaces;

namespace GameShared.Commands.ServerToClient
{
    public class RequestPositionsCommand : IServerToClientCommandHandler
    {
        public ServerToClientEvent CommandType => ServerToClientEvent.GAME_STATE;
        public int PacketSize => 1; // 1 байт - только команда

        public static Dictionary<string, int> FieldOffsets { get; protected set; } = new();

        public void ParseFromBytes(byte[] data) { }

        public byte[] ToBytes()
        {
            return new byte[] { (byte)CommandType };
        }

        public async Task Execute(PaperClient client)
        {
            Console.WriteLine($"[Client] Сервер запросил позиции. Отправляем `SendPositionCommand`...");

            // 🔥 Клиент отправляет свою позицию
            var sendPositionCommand = new SendPositionCommand(client.PlayerId, client.PlayerData.X, client.PlayerData.Z, client.PlayerData.Direction);
            await client.SendCommand(sendPositionCommand);
        }
    }
}