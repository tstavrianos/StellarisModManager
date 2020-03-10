using System;
using Chromely.Core.Logging;
using Serilog;
using Serilog.Exceptions;
using Logger = Serilog.Core.Logger;

namespace StellarisModManager.Blazor
{
    public class MySeriLogger : IChromelyLogger, IDisposable
    {
        private readonly Logger _logger;
        
        public MySeriLogger() {
            this._logger = new LoggerConfiguration()//
#if DEBUG
                .MinimumLevel.Debug()//
                .Enrich.WithExceptionDetails()//
#else
                .MinimumLevel.Information()//
#endif
                .Enrich.FromLogContext()//
                .WriteTo.File("chromely.log")//
                .CreateLogger();//
        }
        
        #region Implementation of IChromelyLogger

        public void Info(string message)
        {
            this._logger.Information(message);
        }

        public void Verbose(string message)
        {
            this._logger.Verbose(message);
        }

        public void Debug(string message)
        {
            this._logger.Debug(message);
        }

        public void Warn(string message)
        {
            this._logger.Warning(message);
        }

        public void Critial(string message)
        {
            this._logger.Fatal(message);
        }

        public void Fatal(string message)
        {
            this._logger.Fatal(message);
        }

        public void Error(string message)
        {
            this._logger.Error(message);
        }

        public void Error(Exception exception)
        {
            this._logger.Error(exception, "Error");
        }

        public void Error(Exception exception, string message)
        {
            this._logger.Error(exception, message);
        }

        #endregion

        public void Dispose()
        {
            this._logger.Dispose();
        }
    }
}
