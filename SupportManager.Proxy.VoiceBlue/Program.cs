using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SupportManager.Proxy.VoiceBlue
{
    public class Program
    {
        private static readonly Regex LoginRegex = new(@"Login:\s*");
        private static readonly Regex PasswordRegex = new(@"Password:\s*");
        private static readonly MemoryPool<byte> MemoryPool = MemoryPool<byte>.Shared;
        private static readonly byte[] NewLine = Encoding.ASCII.GetBytes("\r\n");
        private static readonly byte[] Ok = Encoding.ASCII.GetBytes("OK");

        private static string RemoteHost;
        private static int RemotePort;
        private static string RemoteUserName;
        private static string RemotePassword;
        private static int LocalPort;
        private static int SimSlot;
        private static byte[] Prefix;

        public static async Task Main(string[] args)
        {
            if (args.Length < 6)
            {
                await Console.Error.WriteLineAsync("Insufficient arguments supplied, expected:");
                await Console.Error.WriteLineAsync(
                    "\t<remote host> <remote port> <remote username> <remote password> <sim slot number> <listening port>");

                return;
            }

            RemoteHost = args[0];
            RemotePort = int.Parse(args[1]);
            RemoteUserName = args[2];
            RemotePassword = args[3];
            SimSlot = int.Parse(args[4]);
            LocalPort = int.Parse(args[5]);

            Prefix = Encoding.ASCII.GetBytes($"AT&G{SimSlot:00}=");

            var listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, LocalPort));

            Console.WriteLine($"Listening on port {LocalPort}");

            listenSocket.Listen(10);

            while (true)
            {
                var socket = await listenSocket.AcceptAsync();
                _ = ProxyCommands(socket);
            }
        }

        private static async Task ProxyCommands(Socket socket)
        {
            var client = new TcpClient();
            await client.ConnectAsync(RemoteHost, RemotePort);

            Console.WriteLine($"Connected to {RemoteHost}:{RemotePort}");

            var stream = client.GetStream();

            await LoginToVoiceBlue(stream);

            Task.Factory.StartNew(() => ProcessLinesAsync(client.Client, ProcessVoiceBlue(socket)));
            await ProcessLinesAsync(socket, ForwardToVoiceBlue(stream));
        }

        private static async Task LoginToVoiceBlue(NetworkStream stream)
        {
            async ValueTask SendString(string value)
            {
                using var mem = MemoryPool.Rent();
                var len = Encoding.UTF8.GetBytes(value, mem.Memory.Span);
                await stream.WriteAsync(mem.Memory.Slice(0, len));
                await stream.WriteAsync(NewLine.AsMemory());
            }

            async ValueTask ReadUntilMatch(Regex regex)
            {
                var offset = 0;
                using var lease = MemoryPool.Rent();
                var mem = lease.Memory;

                while (true)
                {
                    var len = await stream.ReadAsync(mem[offset..]);
                    var temp = mem[offset..(offset + len)];
                    var nl = temp.Span.LastIndexOf(NewLine);

                    if (nl != -1)
                    {
                        temp = temp[(nl + 1)..];
                        mem = mem[(nl + 1)..];
                        len -= nl + 1;
                    }

                    if (regex.IsMatch(Encoding.UTF8.GetString(temp.Span)))
                    {
                        break;
                    }

                    offset += len;
                }
            }

            await ReadUntilMatch(LoginRegex);
            await SendString(RemoteUserName);

            await ReadUntilMatch(PasswordRegex);
            await SendString(RemotePassword);

            var offset = 0;
            using var lease = MemoryPool.Rent();
            var mem = lease.Memory;

            while (true)
            {
                var len = await stream.ReadAsync(mem[offset..]);
                if (mem.Span.IndexOf(Ok.AsSpan()) != -1) break;

                offset += len;
            }

            Console.WriteLine("Logged in");
        }

        private delegate ValueTask Process(in ReadOnlySequence<byte> line);

        private static Process ForwardToVoiceBlue(Stream stream)
        {
            return (in ReadOnlySequence<byte> buffer) =>
            {
                stream.Write(Prefix.AsSpan());

                Console.Write("Forwarding: ");
                foreach (var segment in buffer)
                {
                    Console.Write(Encoding.UTF8.GetString(segment.Span));
                    stream.Write(segment.Span);
                }

                Console.WriteLine();

                stream.Write(NewLine.AsSpan());

                return ValueTask.CompletedTask;
            };
        }

        private static Process ProcessVoiceBlue(Socket clientSocket)
        {
            async ValueTask ProxyLine(string line)
            {
                if (line.StartsWith("<++")) return;

                if (line.StartsWith("-->") || line.StartsWith("ERROR"))
                {
                    var idx = line.IndexOf(' ');
                    while (idx < line.Length)
                    {
                        if (line[idx] != ' ') break;

                        idx++;
                    }

                    using var mem = MemoryPool.Rent();
                    var len = Encoding.UTF8.GetBytes(line.AsSpan(idx), mem.Memory.Span);

                    await clientSocket.SendAsync(mem.Memory.Slice(0, len), SocketFlags.None);
                    await clientSocket.SendAsync(NewLine.AsMemory(), SocketFlags.None);
                }
                else
                {
                    Console.WriteLine($"Dropping response: {line}");
                }
            }

            return (in ReadOnlySequence<byte> line) =>
            {
                Console.Write("From VoiceBlue: ");
                Console.Write(Encoding.UTF8.GetString(line));
                Console.WriteLine();

                Console.WriteLine($"Hex: {string.Join(",", line.ToArray().Select(c => $"{c:X}"))}");

                return ProxyLine(Encoding.UTF8.GetString(line).TrimStart());
            };
        }

        private static async Task ProcessLinesAsync(Socket socket, Process callback)
        {
            Console.WriteLine($"[{socket.RemoteEndPoint}]: connected");

            // Create a PipeReader over the network stream
            var stream = new NetworkStream(socket);
            var reader = PipeReader.Create(stream);

            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;

                while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
                {
                    // Process the line.
                    await callback(line);
                }

                // Tell the PipeReader how much of the buffer has been consumed.
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming.
                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete.
            await reader.CompleteAsync();

            Console.WriteLine($"[{socket.RemoteEndPoint}]: disconnected");
        }

        private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            // Look for a EOL in the buffer.
            var cr = buffer.PositionOf((byte) '\r');
            var nl = buffer.PositionOf((byte) '\n');

            if (cr != null)
            {
                var next = buffer.GetPosition(1, cr.Value);
                if (!next.Equals(nl)) cr = null;
            }

            if (nl == null)
            {
                line = default;
                return false;
            }

            // Skip the line + the \n.
            line = buffer.Slice(0, cr ?? nl.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, nl.Value));
            return true;
        }
    }
}
