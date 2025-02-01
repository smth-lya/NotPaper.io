using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameShared.Commands
{
    /// <summary>
    /// Абстрактный базовый класс для всех команд (как клиент-серверных, так и сервер-клиент).
    /// </summary>
    public abstract class BaseCommand<TExecutor, TCommand> 
        where TExecutor : class 
        where TCommand : Enum
    {
        /// <summary>
        /// Тип команды.
        /// </summary>
        public abstract TCommand CommandType { get; }

        /// <summary>
        /// Размер пакета данных в байтах.
        /// </summary>
        public abstract int PacketSize { get; }

        /// <summary>
        /// Десериализует данные из массива байтов.
        /// </summary>
        public abstract void ParseFromBytes(byte[] data);

        /// <summary>
        /// Сериализует объект в массив байтов.
        /// </summary>
        public abstract byte[] ToBytes();

        /// <summary>
        /// Выполняет команду в контексте переданного объекта.
        /// Например, на клиенте (`PaperClient`) или сервере (`PaperServer`).
        /// </summary>
        public abstract Task ExecuteAsync(TExecutor executor);
    }
}
