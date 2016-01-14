using System;

namespace MYCroes.ATCommands
{
    public class ATCommandArgument
    {
        public string FormattedArgument { get; }

        private ATCommandArgument(string stringArgument)
        {
            FormattedArgument = $"\"{stringArgument}\"";
        }

        private ATCommandArgument(int intArgument)
        {
            FormattedArgument = intArgument.ToString();
        }

        public static implicit operator ATCommandArgument(string stringArgument)
        {
            return new ATCommandArgument(stringArgument);
        }

        public static implicit operator ATCommandArgument(int intArgument)
        {
            return new ATCommandArgument(intArgument);
        }

        public static implicit operator ATCommandArgument(Enum enumArgument)
        {
            return new ATCommandArgument((int)(object)enumArgument);
        }

        public override string ToString()
        {
            return FormattedArgument;
        }
    }
}