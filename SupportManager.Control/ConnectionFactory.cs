namespace SupportManager.Control
{
    public static class ConnectionFactory
    {
        public static IConnection CreateConnection(string connectionString)
        {
            return connectionString.StartsWith(TcpConnectionFactory.Proto)
                ? (IConnection) TcpConnectionFactory.CreateFromString(connectionString)
                : new SerialPortConnection(SerialPortFactory.CreateFromConnectionString(connectionString));
        }
    }
}