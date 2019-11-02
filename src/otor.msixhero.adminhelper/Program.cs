using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using otor.msixhero.lib;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.adminhelper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(string.Join(" ", args));
            try
            {
                if (args.Length > 0 && args[0] == "--selfElevate")
                {
                    StartPipe().GetAwaiter().GetResult();
                }
                else
                {
                    Environment.ExitCode = 1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task StartPipe()
        {
            try
            {
                var server = new ClientCommandRemoting(new ProcessManager()).GetServerInstance(new AppxPackageManager());
                await server.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadKey();
        }
    }
}
