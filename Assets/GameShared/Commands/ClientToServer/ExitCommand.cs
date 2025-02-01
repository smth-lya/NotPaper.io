using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using GameShared;
using GameShared.Commands.ServerToClient;
using UnityEngine;

namespace GameShared.Commands.ClientToServer
{
    /// <summary>
    /// Команда выхода игрока из игры.
    /// </summary>
    public sealed class ExitCommand : ClientToServerCommand
    {
        private static Dictionary<string, int> _fieldOffsets = new Dictionary<string, int>()
        {
            { nameof(PlayerId), 1 }
        };

        public override ClientToServerEvent CommandType => ClientToServerEvent.EXIT;
        public override int PacketSize => sizeof(byte) + sizeof(int); // 1 байт - команда, 4 байта - PlayerId

        public int PlayerId { get; private set; }

        public ExitCommand() { }

        public ExitCommand(int playerId) : this()
        {
            PlayerId = playerId;
        }

        /// <summary>
        /// Парсит данные из массива байтов для инициализации команды.
        /// </summary>
        /// <param name="data">Массив байтов.</param>
        /// <exception cref="ArgumentException">Выбрасывается, если массив недостаточной длины.</exception>
        public override void ParseFromBytes(byte[] data)
        {
            if (data == null || data.Length < PacketSize)
            {
                throw new ArgumentException("Некорректный формат данных для парсинга ExitCommand.", nameof(data));
            }

            PlayerId = BitConverter.ToInt32(data, _fieldOffsets[nameof(PlayerId)]);
        }

        /// <summary>
        /// Преобразует команду в массив байтов.
        /// </summary>
        /// <returns>Массив байтов.</returns>
        public override byte[] ToBytes()
        {
            var result = new byte[PacketSize];
            result[0] = (byte)CommandType;
            BitConverter.GetBytes(PlayerId).CopyTo(result, _fieldOffsets[nameof(PlayerId)]);
            return result;
        }

        /// <summary>
        /// Выполняет логику команды выхода.
        /// </summary>
        /// <param name="server">Экземпляр сервера.</param>
        /// <param name="clientSocket">Сокет клиента.</param>
        /// <returns>Задача выполнения команды.</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если server или clientSocket равны null.</exception>
        public override async Task ExecuteAsync(PaperServer server, Socket clientSocket)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            if (clientSocket == null) throw new ArgumentNullException(nameof(clientSocket));

            Debug.Log($"Игрок {PlayerId} выходит из игры...");

            if (server.Players.TryRemove(PlayerId, out _))
            {
                Debug.Log($"Игрок {PlayerId} успешно удалён.");

                // Уведомляем всех игроков о выходе
                var playerExitCommand = new PlayerExitCommand(PlayerId);
                await server.BroadcastAsync(playerExitCommand.ToBytes());
            }
            else
            {
                Debug.Log($"Ошибка: Игрок {PlayerId} не найден.");
            }

            try
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException ex)
            {
                Debug.Log($"Ошибка при закрытии сокета: {ex.Message}");
            }
            finally
            {
                clientSocket.Close();
            }
        }
    }
}
