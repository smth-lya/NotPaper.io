using GameShared;
using UnityEngine;
using System.Collections.Generic;
using System;
using GameShared.Commands.ClientToServer;
using GameShared.Commands.ServerToClient;
using System.Net;
using UnityEngine.SceneManagement;
using TMPro;

public class Server : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ipText;
    [SerializeField] private TextMeshProUGUI _portText;

    private PaperServer _server;

    private static Server s_Instance;
    public static Server Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindAnyObjectByType<Server>();
                if (s_Instance == null)
                {
                    var obj = new GameObject();
                    obj.name = typeof(Server).Name;
                    s_Instance = obj.AddComponent<Server>();
                }
            }

            return s_Instance;
        }
    }

    public int Port { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void Host()
    {
        Port = Convert.ToInt32("6767");

        Debug.Log(Port);

        _server = new PaperServer(Port, 4, new List<Func<ClientToServerCommand>>
        {
            () => new JoinCommand(),
            () => new MoveCommand(),
            () => new ExitCommand(),
            () => new SendPositionCommand()
        });

       _server.StartAsync();
       
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }

    public void Connect()
    {
        Port = Convert.ToInt32("6767");

        Debug.Log(Port);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
