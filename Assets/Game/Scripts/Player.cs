using System.Collections.Generic;
using System.Net;
using UnityEngine;
using GameShared;
using System;
using GameShared.Commands.ServerToClient;
using System.Linq;
using GameShared.Entity;
using System.Threading.Tasks;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerMovement _playerBotPrefab;

    private PlayerMovement _movement;
    private PlayerTrail _trail;
    private PaperClient _client;

    private Dictionary<int, PlayerMovement> _players = new();
    private int _playerId;

    private async void Awake()
    {
        _client = new PaperClient(IPAddress.Loopback, Server.Instance.Port, new List<Func<ServerToClientCommand>>()
        {
            () => new GameStateCommand(),
            () => new RequestPositionsCommand(),
            () => new PlayerJoinCommand(),
            () => new PlayerMoveCommand(),
            () => new PlayerExitCommand(),
            () => new WelcomeCommand(),
        });

        _client.OnCommandReceived += HandleCommandReceived;

        try
        {
            await _client.StartAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connection failed: {ex.Message}");
            throw;
        }
    }

    private void HandleCommandReceived(ServerToClientEvent eventArgs, ServerToClientCommand command)
    {
        Debug.Log(eventArgs + " AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAa");

        switch (command)
        {
            case WelcomeCommand welcome:
                UnityMainThreadDispatcher.Instance.Enqueue(() => HandleWelcomeCommand(welcome));
                break;

            case PlayerJoinCommand joinCommand:
                UnityMainThreadDispatcher.Instance.Enqueue(() => HandlePlayerJoin(joinCommand));
                break;

            case GameStateCommand gameStateCommand:
                UnityMainThreadDispatcher.Instance.Enqueue(() => HandleGameState(gameStateCommand));
                break;

            case PlayerMoveCommand moveCommand:
                UnityMainThreadDispatcher.Instance.Enqueue(() => HandlePlayerMovement(moveCommand));
                break;

            default:
                Debug.LogWarning("Received unknown command.");
                break;
        }
    }

    private void HandleWelcomeCommand(WelcomeCommand welcome)
    {
        _playerId = welcome.PlayerId;
        transform.position = welcome.Position;
    }

    private void HandlePlayerJoin(PlayerJoinCommand joinCommand)
    {
    }

    private void HandleGameState(GameStateCommand gameStateCommand)
    {
        UpdateOrSpawnPlayers(gameStateCommand.Players);
    }

    private void HandlePlayerMovement(PlayerMoveCommand moveCommand)
    {
        if (_players.ContainsKey(moveCommand.PlayerId))
        {
            _players[moveCommand.PlayerId].SetMoveDirection(moveCommand.Direction);
        }
    }

    private void UpdateOrSpawnPlayers(List<BasePlayer> players)
    {
        var existingPlayerIds = _players.Keys.ToHashSet();
        var serverPlayerIds = players.Select(p => p.Id).ToHashSet();

        var playersToRemove = existingPlayerIds.Except(serverPlayerIds).ToList();
        foreach (var playerId in playersToRemove)
        {
            Destroy(_players[playerId].gameObject);
            _players.Remove(playerId);
        }

        foreach (var player in players)
        {
            if (_players.ContainsKey(player.Id))
            {
                // Переносим установку позиции в главный поток
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    _players[player.Id].transform.position = player.Position;
                });
            }
            else
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    var playerBot = Instantiate(_playerBotPrefab, new Vector3(player.Position.x, 0, player.Position.z), Quaternion.identity);
                    _players.Add(player.Id, playerBot);
                });
            }
        }
    }

    private void Start()
    {
        _movement = GetComponent<PlayerMovement>();
        _movement.OnDirectionChanged += _client.ChangeDirectionAsync;
    }
}
