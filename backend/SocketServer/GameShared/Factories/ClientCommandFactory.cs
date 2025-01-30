using GameShared;
using GameShared.Commands;
using GameShared.Interfaces;

namespace GameClient;

public class ClientCommandFactory
{
    private readonly Dictionary<ServerToClientEvent, Func<IServerToClientCommandHandler>> _commands = new();

    public ClientCommandFactory()
    {
        RegisterCommand(() => new PlayerMoveCommand());
    }

    public void RegisterCommand(Func<IServerToClientCommandHandler> commandFactory)
    {
        IServerToClientCommandHandler commandInstance = commandFactory();
        _commands[commandInstance.CommandType] = commandFactory;
    }

    public IServerToClientCommandHandler? ParseCommand(byte[] data, PaperClient client)
    {
        ServerToClientEvent type = (ServerToClientEvent)data[0];

        if (_commands.TryGetValue(type, out var commandFactory))
        {
            var command = commandFactory();
            command.ParseFromBytes(data);
            command.Execute(client);
            return command;
        }

        return null;
    }
}