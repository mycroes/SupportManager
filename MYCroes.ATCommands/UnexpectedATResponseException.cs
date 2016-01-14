using System;

namespace MYCroes.ATCommands
{
    public class UnexpectedATResponseException : Exception
    {
        public UnexpectedATResponseException(string input, string expected, int offset)
            : base($"Unexpected input string:\n\t{input}\nExpected:\n\t{expected}\nAt offset {offset}")
        {
            Input = input;
            Expected = expected;
            Offset = offset;
        }

        public string Input { get; }
        public string Expected { get; }
        public int Offset { get; }
    }
}