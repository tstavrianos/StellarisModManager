using System.Linq;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using Chromely;
using Chromely.Core;
using Chromely.Core.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
#if DEBUG
using Serilog.Exceptions;
#endif

namespace StellarisModManager.Blazor
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            SeriLogBootstrap();
            
            var appurls = GetAppUrl();
            
            if (Environment.GetEnvironmentVariable("ASPNETCORE_CHROMELY") != "false") {
                // This is called when we run with Chromely enabled (true by default)

                // Setup the Web Host Builder to listen on a port
                // But only do this when the parent process is launched initially
                // not when Chromely launches itself as a child process
                if (!args.Contains("--ignore-certificate-errors")) {
                    CreateWebHostBuilder(args).UseUrls(appurls).Build().Start();
                }
                ChromelyBootstrap(args, appurls);

            } else {
                // This is called when we just run as a website without Chromely
                CreateWebHostBuilder(args).UseUrls(appurls).Build().Run();
            }
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog();

        private static string[] GetAppUrl() {
            // Default urls to use
            var appurls = new []{ $"http://localhost:{FreeTcpPort()}" };
            // Check to see if the url to use has been specified in the launchSettings.json
            var envurls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (envurls != null) {
                appurls = envurls.Split(";");
            }
            return appurls;
        }
        
        /// <summary> Bootstrap the Chromely browser. </summary>
        private static void ChromelyBootstrap(string[] args, string[] appurls) {
            var config = DefaultConfiguration.CreateForRuntimePlatform();
            config.WindowOptions.Title = "Stellaris Mod Manager";
            config.StartUrl = appurls.First();
#if DEBUG
            config.DebuggingMode = true;
#else
            config.DebuggingMode = false;
#endif

            AppBuilder
                .Create()
                .UseLogger<MySeriLogger>()
                .UseConfiguration<DefaultConfiguration>(config)
                .UseApp<ChromelyBasicApp>()
                .Build()
                .Run(args);
        }

        /// <summary> Find the next free TCP port for dynamic allocation. </summary>
        /// <returns> Free TCP Port. </returns>
        private static int FreeTcpPort() {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try {
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally {
                listener.Stop();
            }
        }

        private static void SeriLogBootstrap()
        {
            Log.Logger = new LoggerConfiguration()//
#if DEBUG
                .MinimumLevel.Debug()//
                .Enrich.WithExceptionDetails()//
#else
                .MinimumLevel.Information()//
#endif
                .Enrich.FromLogContext()//
                .WriteTo.File("app.log")//
                .CreateLogger();//

#if DEBUG
            AppDomain.CurrentDomain.FirstChanceException +=
                (object source, FirstChanceExceptionEventArgs e) =>
                {
                    Log.Error(e.Exception, "Uncaught exception");
                };
#endif

#if DEBUG
            var listener = new SerilogTraceListener.SerilogTraceListener();
            Trace.Listeners.Add(listener);
#endif
        }
    }
    
    
}
