using System;
using Chromely.Core.Logging;
using Serilog;

namespace StellarisModManager.Blazor
{
    public class MySeriLogger : IChromelyLogger
    {
        #region Implementation of IChromelyLogger

        public void Info(string message)
        {
            Log.Information(message);
        }

        public void Verbose(string message)
        {
            Log.Verbose(message);
        }

        public void Debug(string message)
        {
            Log.Debug(message);
        }

        public void Warn(string message)
        {
            Log.Warning(message);
        }

        public void Critial(string message)
        {
            Log.Fatal(message);
        }

        public void Fatal(string message)
        {
            Log.Fatal(message);
        }

        public void Error(string message)
        {
            Log.Error(message);
        }

        public void Error(Exception exception)
        {
            Log.Error(exception, "Error");
        }

        public void Error(Exception exception, string message)
        {
            Log.Error(exception, message);
        }

        #endregion
    }
}
