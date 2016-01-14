using System;
using System.IO;
using System.Text;

namespace MYCroes.ATCommands
{
    public class ATResponseValueReader
    {
        private readonly StringReader reader;
        private ParseState state;

        public ATResponseValueReader(string responseValues)
        {
            reader = new StringReader(responseValues);
            state = ParseState.FieldStart;
        }

        public string ReadString()
        {
            var result = ReadOptString();
            if (result == null) throw new InvalidOperationException("No data read");

            return result;
        }

        public string ReadOptString()
        {
            var result = new StringBuilder();
            int @char;

            do
            {
                switch (state)
                {
                    case ParseState.FieldStart:
                        @char = reader.Read();
                        switch (@char)
                        {
                            case '"':
                                state = ParseState.Field;
                                break;
                            case ',':
                                state = ParseState.FieldStart;
                                return null;
                            case -1:
                                state = ParseState.End;
                                return null;
                            default:
                                throw new InvalidOperationException("Field does not contain a string value.");
                        }
                        break;
                    case ParseState.Field:
                        @char = reader.Read();
                        switch (@char)
                        {
                            case '"':
                                state = ParseState.EndQuote;
                                break;
                            case -1:
                                throw new InvalidOperationException("Field value was not terminated");
                            default:
                                result.Append((char)@char);
                                break;
                        }
                        break;
                    case ParseState.EndQuote:
                        @char = reader.Read();
                        switch (@char)
                        {
                            case -1:
                                state = ParseState.End;
                                return result.ToString();
                            case ',':
                                state = ParseState.FieldStart;
                                break;
                            default:
                                throw new InvalidOperationException("Invalid string field: data found after closing quote.");
                        }
                        break;
                    case ParseState.End:
                        return null;
                    default:
                        throw new InvalidOperationException("Invalid state.");
                }
            } while (state != ParseState.FieldStart);

            return result.ToString();
        }

        public ATResponseValueReader Read<TValue>(out TValue outValue)
        {
            var result = ReadCore<TValue>();
            if (result == null)
                outValue = default(TValue);
            else if (Nullable.GetUnderlyingType(typeof (TValue))?.IsEnum ?? false)
                outValue = (TValue) Enum.Parse(Nullable.GetUnderlyingType(typeof (TValue)), result.ToString());
            else
                outValue = (TValue)result;
            return this;
        }

        private object ReadCore<TValue>()
        {
            if (typeof(TValue) == typeof(string)) return ReadOptString();
            if (typeof(TValue) == typeof(int)) return ReadInt();
            if (typeof(TValue) == typeof(int?)) return ReadOptInt();
            if (typeof(TValue).IsEnum) return ReadInt();
            if (Nullable.GetUnderlyingType(typeof(TValue))?.IsEnum ?? false) return ReadOptInt();
            if (typeof(TValue) == typeof(bool)) return ReadInt() == 1;
            if (typeof(TValue) == typeof(bool?)) return ReadOptInt()?.Equals(1);

            throw new Exception("Unsupported type");
        }

        public int ReadInt()
        {
            return ReadOptInt().Value;
        }

        public int? ReadOptInt()
        {
            StringBuilder result = new StringBuilder();
            int @char;

            do
            {
                switch (state)
                {
                    case ParseState.FieldStart:
                        @char = reader.Read();
                        switch (@char)
                        {
                            case '"':
                                throw new InvalidOperationException("Field contains a string value.");
                            case -1:
                                state = ParseState.End;
                                return null;
                            case ',':
                                return null;
                            default:
                                state = ParseState.Field;
                                result.Append((char)@char);
                                break;
                        }
                        break;
                    case ParseState.Field:
                        @char = reader.Read();
                        switch (@char)
                        {
                            case -1:
                                state = ParseState.End;
                                return int.Parse(result.ToString());
                            case ',':
                                state = ParseState.FieldStart;
                                break;
                            default:
                                result.Append((char)@char);
                                break;
                        }
                        break;
                    case ParseState.End:
                        return null;
                    default:
                        throw new InvalidOperationException("Invalid state.");
                }
            } while (state != ParseState.FieldStart);

            return int.Parse(result.ToString());
        }

        private enum ParseState
        {
            FieldStart,
            Field,
            StartQuote,
            EndQuote,
            End
        }
    }
}