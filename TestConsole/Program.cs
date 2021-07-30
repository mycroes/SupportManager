using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MYCroes.ATCommands.Forwarding;
using MYCroes.ATCommands.SMS;
using SupportManager.Control;

namespace TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = "COM3";

            var p = SerialPortFactory.CreateFromConnectionString(connectionString);

            var cmd = ForwardingStatus.Query(ForwardingReason.Unconditional);

            p.Open();

            var stream = p.BaseStream;

            stream.ReadTimeout = 5000;
            stream.WriteTimeout = 5000;

            var states = new List<ForwardingStatus>();
            await foreach (var line in cmd.Execute(p.BaseStream))
                states.Add(ForwardingStatus.Parse(line));

            foreach (var state in states)
            {
                Console.WriteLine($"Active: {state.Status}, Class: {state.Class}, Number: {state.Number}, NumberType: {state.NumberType}");
            }

            var desired = states.FirstOrDefault(s => s.Class == ForwardingClass.Voice);
            if (desired != null)
            {
                Console.WriteLine("Match");
                Console.WriteLine(
                    $"Active: {desired.Status}, Class: {desired.Class}, Number: {desired.Number}, NumberType: {desired.NumberType}");
            }


            //return ForwardingStatus.Parse(res[0]).Number;

            Console.WriteLine("Done");
            Console.ReadKey();
            //using (var helper = new ATHelper("COM3"))
            //{
            //    helper.ForwardTo("+3162732927");
            //}

            //foreach (var line in SMSCommand.SetMode(SMSMode.Text).Execute(stream))
            //{
            //    Console.WriteLine("Set SMS mode response: {0}", line);
            //}

            //foreach (var line in new SMSCommand("+31627329279", "Test").Execute(stream))
            //{
            //    Console.WriteLine("Send SMS response: {0}", line);
            //}

            //var fwdCmd = ForwardingStatus.Set(ForwardingReason.Unconditional, ForwardingMode.Registration,
            //    "+31186848697", ForwardingPhoneNumberType.WithInternationalAccessCode,
            //    ForwardingClass.Voice);
            //var fwdCmd = ForwardingStatus.Set(ForwardingReason.Unconditional, ForwardingMode.Erasure,
            //    "+31186848697", ForwardingPhoneNumberType.WithInternationalAccessCode,
            //    ForwardingClass.Voice);
            //foreach (var line in fwdCmd.Execute(stream))
            //{
            //    Console.WriteLine(line);
            //}
            //var cmd = ForwardingStatus.Query(ForwardingReason.Unconditional);

            //using (var port = new SerialPort("COM3")
            //{
            //    ReadTimeout = 2000,
            //    WriteTimeout = 2000,
            //    BaudRate = 115200,
            //    DataBits = 8,
            //    StopBits = StopBits.One,
            //    Parity = Parity.None,
            //    Handshake = Handshake.RequestToSend
            //})
            //{
            //    using (var helper = new ATHelper(port))
            //    {
            //        //helper.ForwardTo("+31620491671");
            //        helper.ForwardTo("");
            //        Console.WriteLine("Number is: " + helper.GetForwardedPhoneNumber());
            //    }
            //}
            //Console.Write("Done");
            //Console.ReadKey();
            //return;
            //    try
            //    {
            //        using (var port = new SerialPort("COM3")
            //        {
            //            ReadTimeout = 2000,
            //            WriteTimeout = 2000,
            //            BaudRate = 115200,
            //            DataBits = 8,
            //            StopBits = StopBits.One,
            //            Parity = Parity.None,
            //            Handshake = Handshake.RequestToSend
            //        })
            //        {
            //            port.Open();
            //            var stream = port.BaseStream;
            //            var reader = new StreamReader(stream);
            //            var writer = new StreamWriter(stream) { AutoFlush = true };

            //            writer.NewLine = "\n";
            //            string command = "ATE1";
            //            //string command = "AT+CCFC=0,2";

            //            Console.WriteLine($"Writing {command}");
            //            writer.Write(command + "\r\n");
            //            //writer.Write((char) 0x0d);
            //            //writer.Write((char) 0x0a);

            //            Console.WriteLine("Beginning read");

            //            int c;
            //            while ((c = reader.Read()) != -1)
            //            {
            //                Console.WriteLine(c);
            //            }

            //            port.Close();
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Console.ForegroundColor = ConsoleColor.Red;
            //        Console.WriteLine(e.Message);
            //        Console.ForegroundColor = ConsoleColor.Gray;
            //    }

            //Console.WriteLine("Done");
            //Console.ReadLine();
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
