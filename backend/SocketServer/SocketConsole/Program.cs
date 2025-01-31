using GameShared;
using GameShared.Commands.ClientToServer;
using GameShared.Interfaces;

namespace SocketConsole;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new PaperServer(6677, 4, new List<Func<IClientToServerCommandHandler>>()
        {
            () => new ExitCommand(),
            () => new JoinCommand(),
            () => new MoveCommand()
        });
        
        
        await server.Start();
    }
}