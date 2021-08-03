using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using SupportManager.Proxy.VoiceBlue;

if (args.Length < 6)
{
    await Console.Error.WriteLineAsync("Insufficient arguments supplied, expected:");
    await Console.Error.WriteLineAsync(
        "\t<remote host> <remote port> <remote username> <remote password> <sim slot number> <listening port>");

    return -1;
}

string remoteHost;
int remotePort;
string remoteUserName;
string remotePassword;
int simSlot;
int localPort;
try
{
    remoteHost = args[0];
    remotePort = int.Parse(args[1]);
    remoteUserName = args[2];
    remotePassword = args[3];
    simSlot = int.Parse(args[4]);
    localPort = int.Parse(args[5]);
}
catch (Exception e)
{
    await Console.Error.WriteLineAsync("Exception occurred while parsing arguments.");
    await Console.Error.WriteLineAsync(e.ToString());

    return -1;
}

var hostBuilder = Host.CreateDefaultBuilder(args.Skip(6).ToArray())
    .UseWindowsService()
    .ConfigureLogging(cfg => cfg.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Information))
    .ConfigureServices((context, services) => services.AddHostedService(s =>
        new ProxyService(remoteHost, remotePort, remoteUserName, remotePassword, simSlot, localPort)));
await hostBuilder.Build().RunAsync();
return 0;