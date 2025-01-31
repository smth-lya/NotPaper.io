using System.Net;
using GameShared;
using GameShared.Commands.ServerToClient;
using GameShared.Interfaces;

namespace SocketClient;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new PaperClient(IPAddress.Loopback, 5555, new List<Func<IServerToClientCommandHandler>>()
        {
            () => new WelcomeCommand(),
            () => new PlayerExitCommand(),
            () => new PlayerMoveCommand(),
            () => new PlayerJoinCommand()
        });
        
        await client.Start();
    }
}