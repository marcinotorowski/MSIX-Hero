using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace otor.msixhero.lib.Infrastructure.Helpers
{
    public static class UserHelper
    {
        private static bool? isLocalAdmin;
        public static bool IsAdministrator()
        {
            if (isLocalAdmin.HasValue)
            {
                return isLocalAdmin.Value;
            }

            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            isLocalAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            return isLocalAdmin.Value;
        }

        public static Task<bool> IsAdministratorAsync(CancellationToken cancellationToken)
        {
            if (isLocalAdmin.HasValue)
            {
                return Task.FromResult(isLocalAdmin.Value);
            }

            return Task.Run(() =>
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                isLocalAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                return isLocalAdmin.Value;
            },
            cancellationToken);
        }
    }
}
