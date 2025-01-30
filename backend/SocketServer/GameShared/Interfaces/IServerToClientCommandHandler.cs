namespace GameShared.Interfaces;

public interface IServerToClientCommandHandler
{
    public static Dictionary<string, int> FieldOffsets { get; protected set; } = new();
    
    ServerToClientEvent CommandType { get; }
    int PacketSize { get; } //  Добавляем размер пакета
    void ParseFromBytes(byte[] data);
    byte[] ToBytes();
    Task Execute(PaperClient server);
}