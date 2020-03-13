using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace StellarisModManager.Blazor
{
    public static class ServerAppUtil
    {
        private const int DefaultPort = 5001;
        private const int StartScan = 5050;
        private const int EndScan = 6000;
        private const string ArgumentType = "--type";
     
        public static int AvailablePort
        {
            get
            {
                for (var i = StartScan; i < EndScan; i++)
                {
                    if (IsPortAvailable(i))
                    {
                        return i;
                    }
                }

                return DefaultPort;
            }
        }

        public static bool IsMainProcess(IEnumerable<string> args)
        {
            if (args == null || !args.Any())
            {
                return true;
            }

            return !HasArgument(args, ArgumentType);
        }
  
        public static bool IsPortAvailable(int port)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            return tcpConnInfoArray.All(endpoint => endpoint.Port != port);
        }

        public static void SavePort(string appName, int port)
        {
            // used to pass the port number to chromely child processes
            var mmf = MemoryMappedFile.CreateNew(appName, 4);
            var accessor = mmf.CreateViewAccessor();
            accessor.Write(0, (int)port);
        }

        public static int GetSavedPort(string appName)
        {
            var mmf = MemoryMappedFile.CreateOrOpen(appName, 4);
            var accessor = mmf.CreateViewAccessor();
            return accessor.ReadInt32(0);
        }
        private static bool HasArgument(IEnumerable<string> args, string arg)
        {
            return args.Any(a => a.StartsWith(arg, StringComparison.Ordinal));
        }
    }
}