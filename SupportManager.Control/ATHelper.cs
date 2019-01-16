using System;
using System.IO.Ports;
using System.Linq;
using MYCroes.ATCommands;
using MYCroes.ATCommands.Forwarding;

namespace SupportManager.Control
{
    public class ATHelper : IDisposable
    {
        private readonly SerialPort serialPort;

        public ATHelper(string portConnectionString)
        {
            serialPort = SerialPortFactory.CreateFromConnectionString(portConnectionString);
            serialPort.Open();
        }

        public ATHelper(SerialPort port)
        {
            serialPort = port;
            port.Open();
        }

        public void ForwardTo(string phoneNumberWithInternationalAccessCode)
        {
            var cmd = ForwardingStatus.Set(ForwardingReason.Unconditional, ForwardingMode.Registration,
                phoneNumberWithInternationalAccessCode, ForwardingPhoneNumberType.WithInternationalAccessCode,
                ForwardingClass.Voice);

            Execute(cmd);
        }

        public string GetForwardedPhoneNumber()
        {
            var cmd = ForwardingStatus.Query(ForwardingReason.Unconditional);
            var res = Execute(cmd);

            var active = res.Select(ForwardingStatus.Parse)
                .FirstOrDefault(s => (s.Status & 1) == 1 && s.Class == ForwardingClass.Voice);
            if (active == null) return null;

            return active.NumberType == ForwardingPhoneNumberType.InternationalWithoutPlus
                ? '+' + active.Number
                : active.Number;
        }

        private string[] Execute(ATCommand command)
        {
            var stream = serialPort.BaseStream;
            stream.ReadTimeout = 10000;
            stream.WriteTimeout = 10000;

            return command.Execute(stream);
        }

        public void Dispose()
        {
            if (serialPort.IsOpen) serialPort.Close();

            serialPort.Dispose();
        }
    }
}