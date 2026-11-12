using Game.Transport.Utp;
using System;
using System.Globalization;
using UnityEngine;

namespace Game.Bootstrap
{
    // Command line: -name <player name> -port <port>
    public class LaunchOptions
    {
        public string PlayerName { get; private set; }
        public ushort Port { get; private set; } = UtpTransportAdapter.DefaultPort;

        public static LaunchOptions FromCommandLine() => Parse(Environment.GetCommandLineArgs());

        public static LaunchOptions Parse(string[] args)
        {
            var options = new LaunchOptions();

            // Every option take value, so the last argument can't start one 
            for (int i = 0; i < args.Length - 1; i++)
            {
                string value = args[i + 1];
                switch (args[i].ToLowerInvariant())
                {
                    case "-name":
                        options.PlayerName = value;
                        break;
                    case "-port":
                        if (ushort.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out ushort port))
                            options.Port = port;
                        break;
                }
            }

            return options;
        }
    }
}