using System;

namespace SupportManager.Telegram.Handlers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal class CommandAttribute : Attribute
    {
        public string Verb { get; }

        public CommandAttribute(string verb)
        {
            Verb = verb;
        }
    }
}