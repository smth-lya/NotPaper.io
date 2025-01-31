using GameShared.Commands.ClientToServer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameShared.Commands.ServerToClient
{
    public class RequestPositionsCommand : ServerToClientCommand
    {
        public override ServerToClientEvent CommandType => ServerToClientEvent.GAME_STATE;
        public override int PacketSize => sizeof(byte); // 1 байт - только команда

        public override void ParseFromBytes(byte[] data) { }

        public override byte[] ToBytes()
            => new byte[] { (byte)CommandType };

        public override async Task ExecuteAsync(PaperClient client)
        {
            Console.WriteLine($"[Client] Сервер запросил позиции. Отправляем `SendPositionCommand`...");

            // 🔥 Клиент отправляет свою позицию
            var sendPositionCommand = new SendPositionCommand(client.Context);
            await client.SendCommandAsync(sendPositionCommand);
        }
    }
}