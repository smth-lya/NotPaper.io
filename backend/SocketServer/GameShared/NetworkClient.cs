using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GameShared
{
    public interface INetworkClient
    {
        IPAddress ServerIp { get; }
        int ServerPort { get; }
        Task ConnectAsync();
        Task SendAsync(byte[] data);
        Task<byte[]> ReceiveAsync();
        void Close();
    }

    public class NetworkClient : INetworkClient
    {
        private readonly Socket _socket;
        private readonly EndPoint _serverEP;

        public IPAddress ServerIp { get; }
        public int ServerPort { get; }

        public NetworkClient(IPAddress serverIp, int serverPort)
        {
            ServerIp = serverIp;
            ServerPort = serverPort;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverEP = new IPEndPoint(serverIp, serverPort);
        }

        public async Task ConnectAsync()
        {
            await _socket.ConnectAsync(_serverEP);
        }

        public async Task SendAsync(byte[] data)
        {
            await _socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
        }

        public async Task<byte[]> ReceiveAsync()
        {
            byte[] buffer = new byte[1024];
            int receivedBytes = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            if (receivedBytes > 0)
            {
                Array.Resize(ref buffer, receivedBytes);
                return buffer;
            }
            return Array.Empty<byte>();
        }

        public void Close()
        {
            _socket.Close();
        }
    }
}
