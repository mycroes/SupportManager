using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SupportManager.Control
{
    public class TcpConnection : IConnection
    {
        private readonly string host;
        private readonly int port;
        private readonly TcpClient tcpClient = new TcpClient
        {
            SendTimeout = 10000,
            ReceiveTimeout = 10000
        };

        public TcpConnection(string host, int port)
        {
            this.host = host;
            this.port = port;
        }


        public async Task OpenAsync()
        {
            await tcpClient.ConnectAsync(host, port);
        }

        public Stream GetStream()
        {
            return tcpClient.GetStream();
        }

        public void Close()
        {
            tcpClient.Close();
        }
    }
}