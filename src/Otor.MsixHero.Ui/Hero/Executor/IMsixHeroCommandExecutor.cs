using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Otor.MsixHero.Ui.Hero.Commands.Base;
using Otor.MsixHero.Ui.Hero.State;

namespace Otor.MsixHero.Ui.Hero.Executor
{
    public interface IMsixHeroCommandExecutor
    {
        MsixHeroState ApplicationState { get; set; }

        Task Invoke<TCommand>(object sender, TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : UiCommand;

        Task<TResult> Invoke<TCommand, TResult>(object sender, TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : UiCommand<TResult>;
    }
}