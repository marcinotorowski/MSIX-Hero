// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.IO;
using System.Threading.Tasks;

namespace Otor.MsixHero.Cli
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