using System;
using System.Runtime.Serialization;

namespace SupportManager.Telegram.Infrastructure
{
    [Serializable]
    public class IncompleteDataException : Exception
    {
        public IncompleteDataException()
        {
        }

        public IncompleteDataException(string message) : base(message)
        {
        }

        public IncompleteDataException(string message, Exception inner) : base(message, inner)
        {
        }

        protected IncompleteDataException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}