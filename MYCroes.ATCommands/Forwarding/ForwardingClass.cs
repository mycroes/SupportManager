using System;

namespace MYCroes.ATCommands.Forwarding
{
    [Flags]
    public enum ForwardingClass
    {
        None = 0,
        Voice = 1,
        Data = 2,
        Fax = 4,
        Sms = 8,
        SyncDataCircuit = 16,
        AsyncDataCircuit = 32,
        DedicatedPacketAccess = 64,
        DedicatedPADAccess = 128,
        Default = 7
    }
}
