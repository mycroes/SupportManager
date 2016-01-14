using System;
using System.Collections.Generic;
using System.IO;

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

        public string Read()
        {
            char[] buffer = new char[1024];
            int length = reader.Read(buffer, 0, buffer.Length);
            string text = new string(buffer, 0, length);
            LogRead(text);

            return text;
        }

        public string ReadLine()
        {
            string line = reader.ReadLine();
            LogRead(line);

            return line;
        }

        public IEnumerable<string> ReadLines()
        {
            string line;
            do
            {
                line = ReadLine();
                yield return line;
            } while (line != null);
        }

        public void Write(string text)
        {
            LogWrite(text);
            writer.Write(text);
        }

        public void WriteLine(string line)
        {
            LogWrite(line);
            writer.WriteLine(line);
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