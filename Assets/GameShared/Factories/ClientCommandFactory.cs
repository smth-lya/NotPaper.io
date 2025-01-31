using GameShared;
using GameShared.Interfaces;
using System;
using System.Collections.Generic;

namespace GameShared.Factories
{
    public class ClientCommandFactory
    {
        private readonly Dictionary<ServerToClientEvent, Func<IServerToClientCommandHandler>> _commands = new();

        public ClientCommandFactory(IEnumerable<Func<IServerToClientCommandHandler>> commandFactories)
        {
            foreach (var factory in commandFactories)
            {
                RegisterCommand(factory);
            }
        }

        public void RegisterCommand(Func<IServerToClientCommandHandler> commandFactory)
        {
            IServerToClientCommandHandler commandInstance = commandFactory();
            _commands[commandInstance.CommandType] = commandFactory;
        }

        public IServerToClientCommandHandler? ParseCommand(byte[] data, PaperClient client)
        {
            if (data.Length == 0)
            {
                UnityEngine.Debug.Log("Ошибка: пустой пакет данных.");
                return null;
            }

            ServerToClientEvent type = (ServerToClientEvent)data[0];

            if (_commands.TryGetValue(type, out var commandFactory))
            {
                var command = commandFactory();
                command.ParseFromBytes(data);
                //command.Execute(client);
                return command;
            }

            UnityEngine.Debug.Log($"Ошибка: нераспознанная команда {type}");
            return null;
        }
    }
}