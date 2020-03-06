namespace StellarisModManager.Cross
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using Chromely.Core;
    using Chromely.Core.Configuration;
    using Chromely.Core.Helpers;
    using Chromely.Core.Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Serilog;
#if DEBUG
    using Serilog.Exceptions;
    using SerilogTraceListener;
#endif

    internal static class Program
    {
        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        static void Main(string[] args)
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
            var listener = new SerilogTraceListener();
            Trace.Listeners.Add(listener);
#endif
            var hostPort = GetFreeTcpPort();
            var baseAddress = IPAddress.Loopback.ToString();

            var _webApp = new WebHostBuilder()
                .UseSockets()
                .UseStaticWebAssets()
                .UseStartup<Startup>()
                .UseKestrel(options =>
                    {
                        options.Limits.MaxConcurrentConnections = null;
                        options.Listen(IPAddress.Loopback, hostPort);
                    })
                .Build();

            var _cancel = new CancellationTokenSource();
            _webApp.RunAsync(_cancel.Token);

            var startUrl = $"http://{baseAddress}:{hostPort}";
            var config = DefaultConfiguration.CreateForRuntimePlatform();
            config.CefDownloadOptions.DownloadSilently = true;
            config.UrlSchemes.Add(new UrlScheme(DefaultSchemeName.RESOURCE, "http", "localhost", string.Empty, UrlSchemeType.Resource, false));
            config.StartUrl = startUrl;
#if DEBUG
            config.DebuggingMode = true;
#else
            config.DebuggingMode = false;
#endif

            AppBuilder
                .Create()
                .UseLogger<MySeriLogger>()
                .UseConfiguration<DefaultConfiguration>(config)
                .UseApp<MyChromelyApp>()
                .Build()
                .Run(args);

        }
    }
}
