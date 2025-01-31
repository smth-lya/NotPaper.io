using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using GameServer;

namespace GameShared.Interfaces
{

    public interface IClientToServerCommandHandler
    {
        public static Dictionary<string, int> FieldOffsets { get; protected set; } = new();
        ClientToServerEvent CommandType { get; }
        int PacketSize { get; } //  Добавляем размер пакета
        void ParseFromBytes(byte[] data);
        byte[] ToBytes();
        // Execute - это наше ответное действие на пришедшую команду, на пришедшее сообщение
        // Например, нам пришло событие, что игрок один сменил направление. В ответ на эту команду мы должны оповестить
        // всех игроков об этом
        Task Execute(PaperServer server, Socket clientSocket);
    }
}