using System.Threading.Tasks;
using GameShared;
using GameShared.Commands;
/// <summary>
/// Базовый класс для всех команд, отправляемых с сервера клиенту.
/// </summary>
public abstract class ServerToClientCommand : BaseCommand<PaperClient, ServerToClientEvent>
{
    /// <summary>
    /// Выполняет команду на клиенте.
    /// </summary>
    public override abstract Task ExecuteAsync(PaperClient client);
}