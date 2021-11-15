using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Otor.MsixHero.Cli.Executors.Edit.Bulk
{
    internal class NativeMethods
    {
        public static string[] ConvertArgumentsStringToArray(string argumentsAsString)
        {
            var pointerToSplitArgs = CommandLineToArgvW(argumentsAsString, out var numberOfArguments);

            if (pointerToSplitArgs == IntPtr.Zero)
            {
                throw new ArgumentException("Unable to split argument.", new Win32Exception());
            }

            var arguments = new string[numberOfArguments];

            try
            {
                for (var i = 0; i < numberOfArguments; i++)
                {
                    arguments[i] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(pointerToSplitArgs, i * IntPtr.Size));
                }

                return arguments;
            }
            finally
            {
                // Free memory obtained by CommandLineToArgW.
                LocalFree(pointerToSplitArgs);
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine,  out int pNumArgs);
    }
}
