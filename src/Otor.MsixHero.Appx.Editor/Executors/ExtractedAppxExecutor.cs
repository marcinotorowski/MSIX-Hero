using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Editor.Commands;
using Otor.MsixHero.Appx.Editor.Executors.Concrete;

namespace Otor.MsixHero.Appx.Editor.Executors
{
    public abstract class ExtractedAppxExecutor<T> : IAppxEditCommandExecutor<T> where T : IAppxEditCommand
    {
        protected readonly DirectoryInfo Directory;

        protected ExtractedAppxExecutor(DirectoryInfo directory)
        {
            this.Directory = directory;
        }

        protected ExtractedAppxExecutor(string directory) : this(new DirectoryInfo(directory))
        {
        }
        
        public abstract Task Execute(T command, CancellationToken cancellationToken = default);

        protected string ResolvePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return relativePath;
            }

            return relativePath.Replace('/', '\\').Replace("%20", " ");
        }
    }
}
