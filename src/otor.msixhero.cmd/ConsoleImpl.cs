using System;
using System.IO;
using System.Threading.Tasks;

namespace otor.msixhero.cmd
{
    public class ConsoleImpl : IConsole
    {
        private readonly TextWriter output;
        private readonly TextWriter errorOut;

        public ConsoleImpl(TextWriter output, TextWriter errorOut)
        {
            this.output = output;
            this.errorOut = errorOut;
        }

        public async Task WriteError(string error)
        {
            var originalBackground = Console.BackgroundColor;
            var originalForeground = Console.ForegroundColor;

            try
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                await this.errorOut.WriteAsync(error).ConfigureAwait(false);
            }
            finally
            {
                Console.BackgroundColor = originalBackground;
                Console.ForegroundColor = originalForeground;
                await this.output.WriteLineAsync();
            }
        }

        public async Task WriteSuccess(string success)
        {
            var originalBackground = Console.BackgroundColor;
            var originalForeground = Console.ForegroundColor;

            try
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.White;
                await this.output.WriteAsync(success).ConfigureAwait(false);
            }
            finally
            {
                Console.BackgroundColor = originalBackground;
                Console.ForegroundColor = originalForeground;
                await this.output.WriteLineAsync();
            }
        }

        public async Task WriteWarning(string warn)
        {
            var originalBackground = Console.BackgroundColor;
            var originalForeground = Console.ForegroundColor;

            try
            {
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.ForegroundColor = ConsoleColor.White;
                await this.output.WriteAsync(warn).ConfigureAwait(false);
            }
            finally
            {
                Console.BackgroundColor = originalBackground;
                Console.ForegroundColor = originalForeground;
                await this.output.WriteLineAsync();
            }
        }

        public Task WriteInfo(string info)
        {
            return this.output.WriteLineAsync(info);
        }
    }
}