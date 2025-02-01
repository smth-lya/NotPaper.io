using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using GameShared.Entity;

namespace GameShared.Commands.ServerToClient
{
    public sealed class GameStateCommand : ServerToClientCommand
    {
        private static readonly Dictionary<string, int> FieldOffsets = new()
        {
            { "PlayerId", 1 },
            { "X", 5 },
            { "Z", 9 },
            { "DirX", 13 },
            { "DirZ", 17 }
        };

        private int _playerDataSize = sizeof(int) + sizeof(float) * 4;

        public override ServerToClientEvent CommandType => ServerToClientEvent.GAME_STATE;
        public override int PacketSize => sizeof(byte) + Players.Count * _playerDataSize;

        public List<BasePlayer> Players { get; private set; } = new();


        public GameStateCommand() { }

        public GameStateCommand(List<BasePlayer> players)
        {
            Players = players;
        }

        public override void ParseFromBytes(byte[] data)
        {
            Players.Clear();

            int count = (data.Length - 1) / _playerDataSize;
            for (int i = 0; i < count; i++)
            {
                int playerId = BitConverter.ToInt32(data, FieldOffsets["PlayerId"] + i * _playerDataSize);

                var position = new Vector3()
                {
                    x = BitConverter.ToSingle(data, FieldOffsets["X"] + i * _playerDataSize),
                    z = BitConverter.ToSingle(data, FieldOffsets["Z"] + i * _playerDataSize)
                };

                var direction = new Vector3()
                {
                    x = BitConverter.ToSingle(data, FieldOffsets["DirX"] + i * _playerDataSize),
                    z = BitConverter.ToSingle(data, FieldOffsets["DirZ"] + i * _playerDataSize)
                };

                Players.Add(new BasePlayer() { Id = playerId, Position = position, Direction = direction });
            }
        }

        public override byte[] ToBytes()
        {
            byte[] result = new byte[PacketSize];
            result[0] = (byte)CommandType;

            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                BitConverter.GetBytes(player.Id).CopyTo(result, FieldOffsets["PlayerId"] + i * _playerDataSize);

                BitConverter.GetBytes(player.Position.x).CopyTo(result, FieldOffsets["X"] + i * _playerDataSize);
                BitConverter.GetBytes(player.Position.z).CopyTo(result, FieldOffsets["Z"] + i * _playerDataSize);
                
                BitConverter.GetBytes(player.Direction.x).CopyTo(result, FieldOffsets["DirX"] + i * _playerDataSize);
                BitConverter.GetBytes(player.Direction.z).CopyTo(result, FieldOffsets["DirZ"] + i * _playerDataSize);
            }

            return result;
        }

        public override Task ExecuteAsync(PaperClient client)
        {
            Debug.Log("[Client] Получен `GameStateCommand`, обновляем позиции игроков...");

            return Task.CompletedTask;
        }
    }
}
