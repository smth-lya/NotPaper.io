using System.Collections.Generic;
using System.Net;
using UnityEngine;
using GameShared;
using System;
using GameShared.Interfaces;
using GameShared.Commands.ServerToClient;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject _playerBotPrefab;

    private PlayerMovement _movement;
    private PlayerTrail _trail;
    private PaperClient _client;


    private async void Awake()
    {
        _client = new PaperClient(IPAddress.Loopback, 6677, new List<Func<IServerToClientCommandHandler>>()
        {
            () => new PlayerJoinCommand(),
            () => new PlayerMoveCommand(),
            () => new PlayerExitCommand(),
            () => new WelcomeCommand(),
        });
        _client.OnCommandReceived += _client_OnCommandReceived;
        try
        {
            await _client.Start();

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            throw;
        }

    }

    private void _client_OnCommandReceived(ServerToClientEvent arg1, IServerToClientCommandHandler arg2)
    {
        if (arg2 is PlayerJoinCommand joinCommand)
        {
            var position = UnityEngine.Random.insideUnitCircle;

            Instantiate(_playerBotPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity);
        }
    }

    private void Update()
    {
        
    }
}
