using System;
using System.Collections.Generic;
using System.Linq;

namespace MYCroes.ATCommands
{
    public class ATCommandException : Exception
    {
        public string[] Lines { get; }

        public ATCommandException(string[] lines)
            : base("An AT Command exception occured:\n\t" + string.Join("\n\t", lines))
        {
            Lines = lines;
        }

        public ATCommandException(IEnumerable<string> lines)
            : this(lines.ToArray())
        {
        }
    }
}