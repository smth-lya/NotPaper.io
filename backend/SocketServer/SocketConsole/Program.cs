using GameShared;
using GameShared.Commands.ClientToServer;

namespace SocketConsole;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new PaperServer(6677, 4, new List<Func<ClientToServerCommand>>
        {
            () => new JoinCommand(),
            () => new MoveCommand(),
            () => new ExitCommand(),
            () => new SendPositionCommand(),
        });

        await server.StartAsync();
    }
}