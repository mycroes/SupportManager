using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using MYCroes.ATCommands;
using MYCroes.ATCommands.Forwarding;

namespace SupportManager.Control
{
    public class ATHelper : IDisposable
    {
        private readonly IConnection connection;

        public ATHelper(string portConnectionString)
        {
            connection = ConnectionFactory.CreateConnection(portConnectionString);
        }

        public ATHelper(SerialPort port)
        {
            connection = new SerialPortConnection(port);
        }

        public async Task OpenAsync()
        {
            await connection.OpenAsync();
        }

        public async Task ForwardTo(string phoneNumberWithInternationalAccessCode)
        {
            var cmd = ForwardingStatus.Set(ForwardingReason.Unconditional, ForwardingMode.Registration,
                phoneNumberWithInternationalAccessCode, ForwardingPhoneNumberType.WithInternationalAccessCode,
                ForwardingClass.Voice);

            await foreach (var _ in Execute(cmd))
            {
            }
        }

        public async Task<string> GetForwardedPhoneNumber()
        {
            var cmd = ForwardingStatus.Query(ForwardingReason.Unconditional);

            ForwardingStatus active = null;
            await foreach (var line in Execute(cmd))
            {
                active = ForwardingStatus.Parse(line);
                if ((active.Status & 1) == 1 && active.Class == ForwardingClass.Voice)
                    break;
            }

            if (active == null) return null;

            return active.NumberType == ForwardingPhoneNumberType.InternationalWithoutPlus
                ? '+' + active.Number
                : active.Number;
        }

        private IAsyncEnumerable<string> Execute(ATCommand command)
        {
            var stream = connection.GetStream();

            return command.Execute(stream);
        }

        public void Dispose()
        {
            connection.Close();
            if (connection is IDisposable disposable) disposable.Dispose();
        }
    }
}