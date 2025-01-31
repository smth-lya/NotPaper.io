using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using GameShared.Entity;

namespace GameShared.Commands.ServerToClient
{
    /// <summary>
    /// Команда приветствия нового игрока, содержащая его ID, позицию и направление.
    /// </summary>
    public sealed class WelcomeCommand : ServerToClientCommand
    {
        private static readonly Dictionary<string, int> FieldOffsets = new()
        {
            { "PlayerId", 1 },
            { "X", 5 },
            { "Z", 9 },
            { "DirX", 13 },
            { "DirZ", 17 }
        };

        public override ServerToClientEvent CommandType => ServerToClientEvent.WELCOME;
        public override int PacketSize => sizeof(byte) + sizeof(int) + sizeof(float) * 4; 

        public int PlayerId { get; }
     
        public Vector3 Position { get; private set; }
        public Vector3 Direction { get; private set; }


        /// <summary>
        /// Создает новый экземпляр команды Welcome.
        /// </summary>
        public WelcomeCommand(int playerId, Vector3 position, Vector3 direction)
        {
            PlayerId = playerId;
            Direction = direction;
        }

        /// <summary>
        /// Десериализует команду из массива байтов.
        /// </summary>
        public override void ParseFromBytes(byte[] data)
        {
            int playerId = BitConverter.ToInt32(data, FieldOffsets["PlayerId"]);

            Position = new Vector3()
            {
                X = BitConverter.ToSingle(data, FieldOffsets["X"]),
                Z = BitConverter.ToSingle(data, FieldOffsets["Z"])
            };

            Direction = new Vector3()
            {
                X = BitConverter.ToSingle(data, FieldOffsets["DirX"]),
                Z = BitConverter.ToSingle(data, FieldOffsets["DirZ"])
            };
        }

        /// <summary>
        /// Сериализует команду в массив байтов.
        /// </summary>
        public override byte[] ToBytes()
        {
            byte[] result = new byte[PacketSize];
            result[0] = (byte)CommandType;
            
            BitConverter.GetBytes(PlayerId).CopyTo(result, FieldOffsets["PlayerId"]);
            
            BitConverter.GetBytes(Position.X).CopyTo(result, FieldOffsets["X"]);
            BitConverter.GetBytes(Position.Z).CopyTo(result, FieldOffsets["Z"]);
            
            BitConverter.GetBytes(Direction.X).CopyTo(result, FieldOffsets["DirX"]);
            BitConverter.GetBytes(Direction.Y).CopyTo(result, FieldOffsets["DirZ"]);
         
            return result;
        }

        /// <summary>
        /// Выполняет команду на клиенте, устанавливая параметры игрока.
        /// </summary>
        public override Task ExecuteAsync(PaperClient client)
        {
            Console.WriteLine($"[Client] WELCOME. PlayerId={PlayerId}, X={Position.X}, Y = {Position.Y}, Z={Position.Y}, Direction={Direction}");

            client.Context = new BasePlayer 
            { 
                Id = PlayerId,
                Position = Position, 
                Direction = Direction 
            };

            return Task.CompletedTask;
        }
    }
}
