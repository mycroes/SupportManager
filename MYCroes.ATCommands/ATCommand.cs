using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MYCroes.ATCommands
{
    public class ATCommand
    {
        private const string AT_OK = "OK";
        private const string AT_ERROR = "ERROR";

        public string Command { get; }
        public IEnumerable<ATCommandArgument> Arguments { get; }

        public ATCommand(string command, IEnumerable<ATCommandArgument> arguments)
        {
            Command = command;
            Arguments = arguments;
        }

        public ATCommand(string command, params ATCommandArgument[] arguments)
            : this(command, arguments.AsEnumerable())
        {
        }

        public static implicit operator string (ATCommand atCommand)
        {
            return atCommand.Command + "=" + string.Join(",", atCommand.Arguments);
        }

        public string[] Execute(Stream stream)
        {
            var chat = new ATChat(stream);

            return ExecuteCore(chat).ToArray();
        }

        protected virtual IEnumerable<string> ExecuteCore(ATChat chat)
        {
            WriteCommand(chat);

            return ReadResponse(chat);
        }

        protected IEnumerable<string> ReadResponse(ATChat chat)
        {
            string responsePrefix = Command + ": ";
            var lines = new List<string>();
            foreach (var line in chat.ReadLines())
            {
                lines.Add(line);
                if (line.StartsWith(responsePrefix))
                {
                    yield return line.Substring(responsePrefix.Length);
                    continue;
                }

                if (line == AT_OK) yield break;

                if (line.EndsWith(AT_ERROR))
                {
                    throw new ATCommandException(lines);
                }
            }

            throw new ATCommandException(lines);
        }

        protected void WriteCommand(ATChat chat)
        {
            chat.WriteLine("AT" + Command + "=" + string.Join(",", Arguments));
        }
    }
}
