using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using otor.msixhero.lib;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.BusinessLayer.Commanding;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Logging;

namespace otor.msixhero.adminhelper
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        static Program()
        {
#if DEBUG
            LogManager.Initialize(MsixHeroLogLevel.Debug);
#else
            LogManager.Initialize(MsixHeroLogLevel.Info);
#endif
        }

        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0] == "--selfElevate")
                {
                    Logger.Debug("Preparing to start the pipe server...");
                    var server = new ClientCommandRemoting(new ProcessManager()).GetServerInstance(new CurrentUserAppxPackageManager(new AppxSigningManager()));
                    server.Start().GetAwaiter().GetResult();
                }
                else
                {
                    Logger.Fatal("Unsupported command line arguments, terminating...");
                    Environment.ExitCode = 1;
                }
            }
            catch (AggregateException e)
            {
                Logger.Fatal(e.GetBaseException(), "Fatal exception, the program will be closed.");
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "Fatal exception, the program will be closed.");
            }

            Logger.Info("Waiting for the user to press a key...");
            Console.ReadKey();
        }
    }
}
