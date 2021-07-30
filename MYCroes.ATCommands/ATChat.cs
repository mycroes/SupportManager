using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MYCroes.ATCommands
{
    public class ATChat
    {
        private readonly TextReader reader;
        private readonly TextWriter writer;

        public ATChat(Stream stream)
        {
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) {AutoFlush = true};
        }

        public async Task<string> Read()
        {
            char[] buffer = new char[1024];
            int length = await reader.ReadAsync(buffer, 0, buffer.Length);
            string text = new string(buffer, 0, length);
            LogRead(text);

            return text;
        }

        public async Task<string> ReadLine()
        {
            string line = await reader.ReadLineAsync();
            LogRead(line);

            return line;
        }

        public async IAsyncEnumerable<string> ReadLines()
        {
            string line;
            do
            {
                line = await ReadLine();
                yield return line;
            } while (line != null);
        }

        public Task Write(string text)
        {
            LogWrite(text);
            return writer.WriteAsync(text);
        }

        public Task WriteLine(string line)
        {
            LogWrite(line);
            return writer.WriteLineAsync(line);
        }

        private void LogRead(string line)
        {
            Console.WriteLine($"RD: {line}");
        }

        private void LogWrite(string line)
        {
            Console.WriteLine($"WR: {line}");
        }
    }
}