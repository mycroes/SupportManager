using System;

namespace SupportManager.Control
{
    public static class TcpConnectionFactory
    {
        public const string Proto = "tcp://";

        public static TcpConnection CreateFromString(string connectionString)
        {
            if (!connectionString.StartsWith(Proto))
            {
                throw new ArgumentException(
                    $"Invalid connectionString specified: {connectionString}, should start with 'tcp://'.",
                    nameof(connectionString));
            }

            var colon = connectionString.IndexOf(':', Proto.Length);
            if (colon == -1)
            {
                throw new ArgumentException(
                    $"Invalid connectionString specified: {connectionString}, should contain ':'.",
                    nameof(connectionString));
            }

            var host = connectionString.Substring(Proto.Length, colon - Proto.Length);

            if (!int.TryParse(connectionString.Substring(colon + 1), out var port))
            {
                throw new ArgumentException(
                    $"Invalid connectionString specified: {connectionString}, couldn't parse port.",
                    nameof(connectionString));
            }

            return new TcpConnection(host, port);
        }
    }
}