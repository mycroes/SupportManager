using System.Collections.Generic;
using System.Threading.Tasks;

namespace MYCroes.ATCommands.SMS
{
    public class SMSCommand : ATCommand
    {
        private const string COMMAND = "+CMGS";
        private const string MODE_COMMAND = "+CMGF";
        private const string MESSAGE_PROMPT = ">";

        private readonly string message;

        public SMSCommand(string phoneNumber, string message)
            : base(COMMAND, phoneNumber)
        {
            this.message = message;
        }

        protected override async IAsyncEnumerable<string> ExecuteCore(ATChat chat)
        {
            await WriteCommand(chat);
            var res = await chat.Read();
            if (res.Trim().StartsWith(MESSAGE_PROMPT))
            {
                await chat.Write(message);
                await chat.Write(new string((char) 26, 1));
            }
            else
            {
                throw new UnexpectedATResponseException(res, MESSAGE_PROMPT, 0);
            }

            await foreach (var line in ReadResponse(chat)) yield return line;
        }

        public static ATCommand SetMode(SMSMode mode)
        {
            return new ATCommand(MODE_COMMAND, mode);
        }
    }
}
