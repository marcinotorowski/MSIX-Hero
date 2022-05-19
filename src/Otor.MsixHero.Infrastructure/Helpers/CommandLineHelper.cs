using System;
using System.Text;

namespace Otor.MsixHero.Infrastructure.Helpers
{
    /// <remarks>Based on https://stackoverflow.com/a/37646895</remarks>
    public class CommandLineHelper
    {
        private static readonly char[] SpecialChars = " \t\n\v\"".ToCharArray();
        /// <summary>
        ///     This routine appends the given argument to a command line such that
        ///     CommandLineToArgvW will return the argument string unchanged. Arguments
        ///     in a command line should be separated by spaces; this function does
        ///     not add these spaces.
        /// </summary>
        /// <param name="argument">Supplies the argument to encode.</param>
        /// <param name="force">
        ///     Supplies an indication of whether we should quote the argument even if it 
        ///     does not contain any characters that would ordinarily require quoting.
        /// </param>
        public static string EncodeParameterArgument(string argument, bool force = false)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(nameof(argument));
            }

            // Unless we're told otherwise, don't quote unless we actually
            // need to do so --- hopefully avoid problems if programs won't
            // parse quotes properly
            if (force == false && argument.Length > 0 && argument.IndexOfAny(SpecialChars) == -1)
            {
                return argument;
            }

            var quoted = new StringBuilder();
            quoted.Append('"');

            var numberBackslashes = 0;

            foreach (var chr in argument)
            {
                switch (chr)
                {
                    case '\\':
                        numberBackslashes++;
                        continue;
                    case '"':
                        // Escape all backslashes and the following
                        // double quotation mark.
                        quoted.Append('\\', numberBackslashes * 2 + 1);
                        quoted.Append(chr);
                        break;
                    default:
                        // Backslashes aren't special here.
                        quoted.Append('\\', numberBackslashes);
                        quoted.Append(chr);
                        break;
                }
                numberBackslashes = 0;
            }

            // Escape all backslashes, but let the terminating
            // double quotation mark we add below be interpreted
            // as a metacharacter.
            quoted.Append('\\', numberBackslashes * 2);
            quoted.Append('"');

            return quoted.ToString();
        }
    }
}