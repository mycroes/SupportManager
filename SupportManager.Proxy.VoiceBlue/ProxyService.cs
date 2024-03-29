﻿using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SupportManager.Proxy.VoiceBlue
{
    internal class ProxyService : BackgroundService
    {
        private readonly ILogger<ProxyService> logger;
        private static readonly Regex LoginRegex = new(@"Login:\s*");
        private static readonly Regex PasswordRegex = new(@"Password:\s*");
        private static readonly MemoryPool<byte> MemoryPool = MemoryPool<byte>.Shared;
        private static readonly byte[] NewLine = Encoding.ASCII.GetBytes("\r\n");
        private static readonly byte[] Ok = Encoding.ASCII.GetBytes("OK");

        private readonly byte[] prefix;

        public string RemoteHost { get; }
        public int RemotePort { get; }
        public string RemoteUserName { get; }
        public string RemotePassword { get; }
        public int SimSlot { get; }
        public int LocalPort { get; }

        public ProxyService(string remoteHost, int remotePort, string remoteUserName, string remotePassword, int simSlot, int localPort, ILogger<ProxyService> logger)
        {
            this.logger = logger;
            RemoteHost = remoteHost;
            RemotePort = remotePort;
            RemoteUserName = remoteUserName;
            RemotePassword = remotePassword;
            SimSlot = simSlot;
            LocalPort = localPort;

            prefix = Encoding.ASCII.GetBytes($"AT&G{SimSlot:00}=");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, LocalPort));

            logger.Log(LogLevel.Information, $"Listening on port {LocalPort}");

            listenSocket.Listen(10);

            await using (stoppingToken.UnsafeRegister(_ => listenSocket.Close(), null))
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var socket = await listenSocket.AcceptAsync();
                    _ = ProxyCommands(socket);
                }
            }
        }

        private async Task ProxyCommands(Socket socket)
        {
            var client = new TcpClient();
            await client.ConnectAsync(RemoteHost, RemotePort);

            logger.Log(LogLevel.Debug, $"Connected to {RemoteHost}:{RemotePort}");

            var stream = client.GetStream();

            await LoginToVoiceBlue(stream);

            _ = Task.Factory.StartNew(async () =>
            {
                await ProcessLinesAsync(client.Client, ProcessVoiceBlue(socket));
                if (socket.Connected) socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            });
            await ProcessLinesAsync(socket, ForwardToVoiceBlue(stream));
            if (client.Client.Connected) client.Client.Shutdown(SocketShutdown.Both);
            client.Client.Close();
        }

        private async Task LoginToVoiceBlue(NetworkStream stream)
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

            logger.Log(LogLevel.Debug, "Logged in");
        }

        private delegate ValueTask Process(in ReadOnlySequence<byte> line);

        private Process ForwardToVoiceBlue(Stream stream)
        {
            return (in ReadOnlySequence<byte> buffer) =>
            {
                stream.Write(prefix.AsSpan());

                logger.Log(LogLevel.Debug, $"Forwarding: {Encoding.UTF8.GetString(buffer)}");

                foreach (var segment in buffer)
                {
                    stream.Write(segment.Span);
                }

                stream.Write(NewLine.AsSpan());

                return ValueTask.CompletedTask;
            };
        }

        private Process ProcessVoiceBlue(Socket clientSocket)
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
                    logger.Log(LogLevel.Debug, $"Dropping response: {line}");
                }
            }

            return (in ReadOnlySequence<byte> line) =>
            {
                logger.Log(LogLevel.Debug, $"From VoiceBlue: {Encoding.UTF8.GetString(line)}");
                logger.Log(LogLevel.Debug, $"Hex: {string.Join(",", line.ToArray().Select(c => $"{c:X}"))}");

                return ProxyLine(Encoding.UTF8.GetString(line).TrimStart());
            };
        }

        private async Task ProcessLinesAsync(Socket socket, Process callback)
        {
            logger.Log(LogLevel.Debug, $"[{socket.RemoteEndPoint}]: connected");

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

            logger.Log(LogLevel.Debug, $"[{socket.RemoteEndPoint}]: disconnected");
        }

        private bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
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