using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;

namespace SupportManager.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseHttpSys(options =>
                {
                    options.Authentication.Schemes = 
                        AuthenticationSchemes.Negotiate | AuthenticationSchemes.NTLM;
                    options.Authentication.AllowAnonymous = false;
                })
                .UseStartup<Startup>()
                .Build();
    }
}
