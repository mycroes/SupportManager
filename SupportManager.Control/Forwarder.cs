using System;
using SupportManager.Contracts;

namespace SupportManager.Control
{
    public class Forwarder : IForwarder
    {
        public void ApplyForward(int id)
        {
            Console.WriteLine($"Applying forward with id {id}");
        }

        public void ReadStatus()
        {
            Console.WriteLine("Read status called");
        }
    }
}