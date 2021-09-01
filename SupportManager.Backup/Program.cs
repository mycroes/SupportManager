using System;
using System.IO;
using System.Threading.Tasks;
using SupportManager.Backup;
using SupportManager.DAL;

const int InvalidArgs = -1;
const int RuntimeError = -2;

static int Usage()
{
    var path = new FileInfo(Environment.GetCommandLineArgs()[0]);
    Console.WriteLine($"Usage: {path.Name} <import|export> <filename>");

    return InvalidArgs;
}

if (args.Length < 1)
{
    Console.WriteLine("Missing action argument");

    return Usage();
}

if (args.Length < 2)
{
    Console.WriteLine("Missing filename argument");

    return Usage();
}

var action = args[0];
var fileName = args[1];
var progress = new Progress<string>(Console.WriteLine);

Func<SupportManagerContext, Task> handler;

switch (action)
{
    case "import":
        handler = (db) => new Importer(db, fileName, progress).Import();
        break;
    case "export":
        handler = (db) => new Exporter(db, fileName, progress).Export();
        break;
    default:
        Console.WriteLine($"Invalid action '{action}'");

        return Usage();
}

try
{
    await handler.Invoke(new SupportManagerContext());
}
catch (Exception e)
{
    Console.WriteLine("Runtime error:");
    Console.WriteLine(e.Message);

    return RuntimeError;
}


return 0;
