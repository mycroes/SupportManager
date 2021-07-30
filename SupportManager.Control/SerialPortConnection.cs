using System;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace SupportManager.Control
{
    public class SerialPortConnection : IConnection, IDisposable
    {
        private readonly SerialPort serialPort;

        public SerialPortConnection(SerialPort serialPort)
        {
            this.serialPort = serialPort;
        }

        public Task OpenAsync()
        {
            serialPort.Open();
            return Task.CompletedTask;
        }

        public Stream GetStream()
        {
            var stream = serialPort.BaseStream;
            stream.ReadTimeout = 10000;
            stream.WriteTimeout = 10000;

            return stream;
        }

        public void Close()
        {
            if (serialPort.IsOpen) serialPort.Close();
        }

        public void Dispose()
        {
            serialPort?.Dispose();
        }
    }
}