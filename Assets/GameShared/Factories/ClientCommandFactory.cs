using GameShared;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameShared.Factories
{
    public class ClientCommandFactory
    {
        private readonly Dictionary<ServerToClientEvent, Func<ServerToClientCommand>> _commands = new();

        public ClientCommandFactory(IEnumerable<Func<ServerToClientCommand>> commandFactories)
        {
            foreach (var factory in commandFactories)
            {
                RegisterCommand(factory);
            }
        }

        public void RegisterCommand(Func<ServerToClientCommand> commandFactory)
        {
            ServerToClientCommand commandInstance = commandFactory();
            _commands[commandInstance.CommandType] = commandFactory;
        }

        public ServerToClientCommand? ParseCommand(byte[] data, PaperClient client)
        {
            if (data.Length == 0)
            {
                Debug.Log("Ошибка: пустой пакет данных.");
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

            Debug.Log($"Ошибка: нераспознанная команда {type}");
            return null;
        }
    }
}