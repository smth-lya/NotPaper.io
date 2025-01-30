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
    void Execute(PaperServer server);
}