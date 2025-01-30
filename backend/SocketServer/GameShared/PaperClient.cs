using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GameShared
{
    public class PaperClient
    {
        private Socket _socket;
        private readonly string _serverIp;
        private readonly int _serverPort;
        private EndPoint _serverEP;

        public PaperClient(string serverIp, int serverPort)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
        }
        
        public void Start()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _serverEP = new IPEndPoint(IPAddress.Parse(_serverIp), _serverPort);

            string joinMessage = "JOIN|Player1";
            _socket.SendTo(Encoding.UTF8.GetBytes(joinMessage), _serverEP);
            Console.WriteLine($"[Client] Отправил: {joinMessage}");
        }
    }
}