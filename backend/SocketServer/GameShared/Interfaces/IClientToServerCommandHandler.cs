using System.Net.Sockets;
using GameServer;

namespace GameShared.Interfaces;

public interface IClientToServerCommandHandler
{
    public static Dictionary<string, int> FieldOffsets { get; protected set; } = new();
    ClientToServerEvent CommandType { get; }
    int PacketSize { get; } //  Добавляем размер пакета
    void ParseFromBytes(byte[] data);
    byte[] ToBytes();
    Task Execute(PaperServer server, Socket clientSocket);
}