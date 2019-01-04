using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Topshelf;

namespace SupportManager.Web
{
    public class Service : ServiceControl
    {
        private readonly string[] args;
        private IWebHost webHost;

        public Service(params string[] args)
        {
            this.args = args;
        }

        public bool Start(HostControl hostControl)
        {
            webHost = WebHost.CreateDefaultBuilder(args)
                .UseHttpSys(options =>
                {
                    options.Authentication.Schemes = AuthenticationSchemes.NTLM | AuthenticationSchemes.Negotiate;
                    options.Authentication.AllowAnonymous = true;
                }).UseStartup<Startup>()
                .UseContentRoot(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)).Build();

            webHost.Start();

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            webHost.Dispose();
            return true;
        }
    }
}
