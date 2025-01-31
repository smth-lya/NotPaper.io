using System;
using System.Collections.Generic;
using System.Net.Sockets;
using GameShared;

namespace GameServer
{
    public class ServerCommandFactory
    {
        private readonly Dictionary<ClientToServerEvent, Func<ClientToServerCommand>> _commands = new();
        private readonly Dictionary<ClientToServerEvent, int> _commandSizes = new();

        public ServerCommandFactory(IEnumerable<Func<ClientToServerCommand>> commandFactories)
        {
            foreach (var factory in commandFactories)
            {
                RegisterCommand(factory);
            }
        }

        public void RegisterCommand(Func<ClientToServerCommand> commandFactory)
        {
            ClientToServerCommand commandInstance = commandFactory();
            _commands[commandInstance.CommandType] = commandFactory;
            _commandSizes[commandInstance.CommandType] = commandInstance.PacketSize;
        }

        public int GetCommandSize(ClientToServerEvent commandType)
        {
            return _commandSizes.TryGetValue(commandType, out int size) ? size : 0;
        }

        public ClientToServerCommand? ParseCommand(byte[] data, Socket clientSocket, PaperServer server)
        {
            ClientToServerEvent type = (ClientToServerEvent)data[0];

            if (_commands.TryGetValue(type, out var commandFactory))
            {
                var command = commandFactory();
                command.ParseFromBytes(data);
                //command.Execute(server, clientSocket);
                return command;
            }

            return null;
        }
    }
}