using System.Threading.Tasks;

namespace Otor.MsixHero.Cli.Executors
{
    public abstract class VerbExecutor<T>
    {
        protected readonly T Verb;
        protected readonly IConsole Console;

        protected VerbExecutor(T verb, IConsole console)
        {
            this.Verb = verb;
            this.Console = console;
        }

        public abstract Task<int> Execute();
    }
}