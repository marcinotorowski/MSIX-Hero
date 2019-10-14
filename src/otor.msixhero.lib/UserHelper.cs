using System;
using System.Security.Principal;

namespace otor.msixhero.lib
{
    public static class UserHelper
    {
        public static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
}
}
