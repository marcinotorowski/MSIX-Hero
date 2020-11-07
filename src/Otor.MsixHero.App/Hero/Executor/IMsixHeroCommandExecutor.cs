using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.App.Hero.Executor
{
    public interface IMsixHeroCommandExecutor
    {
        MsixHeroState ApplicationState { get; set; }

        Task Invoke<TCommand>(object sender, TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : UiCommand;

        Task<TResult> Invoke<TCommand, TResult>(object sender, TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : UiCommand<TResult>;
    }
}