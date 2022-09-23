using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.Appx.Signing.Testing
{
    public interface ISigningTestService
    {
        Task<SignTestResult> VerifyPfx(string pfxPath, SecureString password = default, string timeStampServer = default, CancellationToken cancellation = default);

        Task<SignTestResult> VerifyInstalled(string thumbprint, string timeStampServer = default, CancellationToken cancellation = default);

        Task<SignTestResult> VerifyDeviceGuardSettings(DeviceGuardConfiguration config, string timeStampServer = default, CancellationToken cancellation = default);
    }
}
