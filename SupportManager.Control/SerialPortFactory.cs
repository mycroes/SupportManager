using System;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace SupportManager.Control
{
    public static class SerialPortFactory
    {
        private static readonly Regex ConnectionStringRegex = new Regex(@"^(?:(?:(\w+)=)?(\w+))(?:;(\w+)=(\w+))*$");

        /// <summary>
        /// Creates a <see cref="T:System.IO.Ports.SerialPort" /> from an optional port name and <see cref="T:System.IO.Ports.SerialPort" /> properties with values.
        /// </summary>
        /// <param name="connectionString">A <see cref="T:System.IO.Ports.SerialPort" /> connection string consisting of (optional) port name and semicolon separated properties with value assignments.</param>
        /// <returns><see cref="T:System.IO.Ports.SerialPort" /> with properties set according to supplied values.</returns>
        /// <example>
        /// The following are all valid invocations of <see cref="CreateFromConnectionString"/>:
        /// <code>
        /// SerialPortFactory.CreateFromConnectionString("COM1");
        /// SerialPortFactory.CreateFromConnectionString("PortName=COM1");
        /// SerialPortFactory.CreateFromConnectionString("COM1;BaudRate=115200");
        /// SerialPortFactory.CreateFromConnectionString("PortName=COM1;BaudRate=115200");
        /// SerialPortFactory.CreateFromConnectionString("COM1;BaudRate=115200;DataBits=8;StopBits=One;Parity=None;Handshake=RequestToSend");
        /// </code>
        /// </example>
        public static SerialPort CreateFromConnectionString(string connectionString)
        {
            var match = ConnectionStringRegex.Match(connectionString);
            if (!match.Success) throw new ArgumentException($"Invalid connectionString specified: {connectionString}", nameof(connectionString));

            var firstProp = match.Groups[1].Success ? match.Groups[1].Value : nameof(SerialPort.PortName);
            var firstValue = match.Groups[2].Value;

            var serialPort = new SerialPort();
            try
            {
                SetPropertyValue(serialPort, firstProp, firstValue);

                var propCaptures = match.Groups[3].Captures;
                var valueCaptures = match.Groups[4].Captures;

                for (int i = 0; i < propCaptures.Count; i++)
                    SetPropertyValue(serialPort, propCaptures[i].Value, valueCaptures[i].Value);

                return serialPort;
            }
            catch
            {
                // Cleanup
                serialPort.Dispose();
                throw;
            }
        }

        private static void SetPropertyValue(SerialPort serialPort, string propertyName, string value)
        {
            var propertyInfo = typeof(SerialPort).GetProperty(propertyName) ??
                throw new ArgumentException($"Couldn't find property {propertyName}.", propertyName);
            var typedValue = ConvertValue(value, propertyInfo.PropertyType);

            propertyInfo.SetValue(serialPort, typedValue);
        }

        private static object ConvertValue(string input, Type targetType)
        {
            if (targetType.IsEnum) return Enum.Parse(targetType, input);

            return Convert.ChangeType(input, targetType);
        }
    }
}