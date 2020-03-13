using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Chromely;
using Chromely.Core;
using Chromely.Core.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
#if DEBUG
using Serilog.Exceptions;
#endif

namespace StellarisModManager.Blazor
{
    internal static class Program
    {
        private static Logger _blazorLogger;
        [STAThread]
        private static int Main(string[] args)
        {
            SeriLogBootstrap();
            
            var appName = Assembly.GetEntryAssembly()?.GetName().Name;
            var firstProcess = ServerAppUtil.IsMainProcess(args);
            var port = ServerAppUtil.AvailablePort;
            
            if (firstProcess)
            {
                if (port != -1)
                {
                    // start the kestrel server in a background thread
                    var blazorTask = new Task(() => CreateHostBuilder(args, port).Build().Run(), TaskCreationOptions.LongRunning);
                    blazorTask.Start();

                    // wait till its up
                    while (ServerAppUtil.IsPortAvailable(port))
                    {
                        Thread.Sleep(1);
                    }
                }

                // Save port for later use by chromely processes
                ServerAppUtil.SavePort(appName, port);
            }
            else
            {
                // fetch port number
                port = ServerAppUtil.GetSavedPort(appName);
            }

            if (port == -1) return 1;
            // start up chromely
            var core = typeof(IChromelyConfiguration).Assembly;
            var config = DefaultConfiguration.CreateForRuntimePlatform();
            config.WindowOptions.Title = "Stellaris Mod Manager";
            config.StartUrl = $"http://127.0.0.1:{port}";
#if DEBUG
            config.DebuggingMode = true;
#else
            config.DebuggingMode = false;
#endif

            try
            {
                var builder = AppBuilder.Create();
                builder = builder.UseConfiguration<DefaultConfiguration>(config);
                builder = builder.UseLogger<MySeriLogger>();
                builder = builder.UseApp<ChromelyBasicApp>();
                builder = builder.Build();
                builder.Run(args);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args, int port) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseStartup<Startup>()
                    .UseUrls($"http://127.0.0.1:{port}"))
                .UseSerilog(_blazorLogger)
        ;

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
            /*AppDomain.CurrentDomain.FirstChanceException +=
                (source, e) => Log.Error(e.Exception, "Uncaught exception");*/
            var listener = new SerilogTraceListener.SerilogTraceListener();
            Trace.Listeners.Add(listener);
#endif
            
            _blazorLogger = new LoggerConfiguration()//
#if DEBUG
                .MinimumLevel.Debug()//
                .Enrich.WithExceptionDetails()//
#else
                .MinimumLevel.Information()//
#endif
                .Enrich.FromLogContext()//
                .WriteTo.File("blazor.log")//
                .CreateLogger();//
        }
    }
}
