using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string cmd = "AT+CCFC=0,2";
            var conn = new PhoneConnection("COM3");
            conn.Open();
            conn.Send(cmd);
            Console.WriteLine(conn.Read());
            Console.WriteLine(conn.Read());
            conn.Close();
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }

    public class PhoneConnection
    {
        private readonly SerialPort port;
        private const string Crlf = "\r\n";

        public PhoneConnection(string serialPort)
        {
            port = new SerialPort(serialPort);
        }

        public void Open()
        {
            port.Open();
        }

        public void Close()
        {
            port.Close();
        }

        public void Send(string command)
        {
            LogWrite(command);
            port.Write(command);
            port.Write(Crlf);
        }

        public string Read()
        {
            string result = port.ReadTo(Crlf);
            LogRead(result);
            result = port.ReadTo(Crlf);
            LogRead(result);
            return result;
        }

        private void LogRead(string data)
        {
            Console.WriteLine($"Data read: {data}");
        }

        private void LogWrite(string data)
        {
            Console.WriteLine($"Data written: {data}");
        }

    }
}
