using System.Threading.Tasks;

namespace Otor.MsixHero.Cli
{
    public interface IConsole
    {
        Task WriteError(string error);

        Task WriteWarning(string warn);

        Task WriteInfo(string info);

        Task WriteSuccess(string success);
    }
}