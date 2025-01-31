using System.Net.Sockets;
using System.Threading.Tasks;
using GameShared;
using GameShared.Commands;

/// <summary>
/// Базовый класс для всех команд, отправляемых с клиента на сервер.
/// </summary>
public abstract class ClientToServerCommand : BaseCommand<PaperServer, ClientToServerEvent>
{
    /// <summary>
    /// Выполняет команду на сервере, используя сокет клиента.
    /// </summary>
    public abstract Task ExecuteAsync(PaperServer server, Socket clientSocket);

    /// <summary>
    /// Переопределение ExecuteAsync для совместимости с базовым `BaseCommand`.
    /// По умолчанию, просто вызывает `ExecuteAsync` с `null`-сокетом.
    /// </summary>
    public override async Task ExecuteAsync(PaperServer server)
    {
        await ExecuteAsync(server, null!);
    }
} 
