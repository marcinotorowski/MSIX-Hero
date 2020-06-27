using System.Threading.Tasks;

namespace otor.msixhero.cmd
{
    public interface IConsole
    {
        Task WriteError(string error);

        Task WriteWarning(string warn);

        Task WriteInfo(string info);

        Task WriteSuccess(string success);
    }
}