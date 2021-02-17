using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Otor.MsixHero.App.Services
{
    [Obfuscation(Exclude = true)]
    internal class User32Interop
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();
    }
}
