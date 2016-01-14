using System.Collections.Generic;

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

        protected override IEnumerable<string> ExecuteCore(ATChat chat)
        {
            WriteCommand(chat);
            var res = chat.Read();
            if (res.Trim().StartsWith(MESSAGE_PROMPT))
            {
                chat.Write(message);
                chat.Write(new string((char) 26, 1));
            }
            else
            {
                throw new UnexpectedATResponseException(res, MESSAGE_PROMPT, 0);
            }

            return ReadResponse(chat);
        }

        public static ATCommand SetMode(SMSMode mode)
        {
            return new ATCommand(MODE_COMMAND, mode);
        }
    }
}
