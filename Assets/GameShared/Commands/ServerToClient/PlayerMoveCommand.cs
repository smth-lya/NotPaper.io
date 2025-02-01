using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace GameShared.Commands.ServerToClient
{
    public sealed class PlayerMoveCommand : ServerToClientCommand
    {
        private static readonly Dictionary<string, int> _fieldOffsets = new()
        {
            { "PlayerId", 1 },
            { "DirX", 5 },
            { "DirZ", 9 }
        };
     
        public override ServerToClientEvent CommandType => ServerToClientEvent.PLAYER_MOVE;
        public override int PacketSize => sizeof(byte) + sizeof(int) + sizeof(float) * 2;
       
        public int PlayerId { get; private set; }
        public Vector3 Direction { get; private set; }

        public PlayerMoveCommand() { }

        public PlayerMoveCommand(int playerId, Vector3 direction)
        {
            PlayerId = playerId;
            Direction = direction;
        }

        public override void ParseFromBytes(byte[] data)
        {
            PlayerId = BitConverter.ToInt32(data, _fieldOffsets["PlayerId"]);

            Direction = new Vector3()
            {
                x = BitConverter.ToSingle(data, _fieldOffsets["DirX"]),
                z = BitConverter.ToSingle(data, _fieldOffsets["DirZ"])
            };
        }

        public override byte[] ToBytes()
        {
            byte[] result = new byte[PacketSize];
            result[0] = (byte)CommandType;

            BitConverter.GetBytes(PlayerId).CopyTo(result, _fieldOffsets["PlayerId"]);
            BitConverter.GetBytes(Direction.x).CopyTo(result, _fieldOffsets["DirX"]);
            BitConverter.GetBytes(Direction.z).CopyTo(result, _fieldOffsets["DirZ"]);

            return result;
        }


        public override Task ExecuteAsync(PaperClient client)
        {
            Debug.LogWarning($"Игрок {PlayerId} сменил направление на {Direction}");

            return Task.CompletedTask;
            // 🔥 Unity может подписаться на это событие и обработать его
        }
    }
}