using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Hero.Executor;
using Otor.MsixHero.Ui.Hero.State;
using Prism.Events;

namespace Otor.MsixHero.Ui.Hero
{
    public interface IMsixHeroApplication
    {
        IMsixHeroCommandExecutor CommandExecutor { get; }

        IEventAggregator EventAggregator { get; }

        MsixHeroState ApplicationState { get; }

        IConfigurationService ConfigurationService { get; }
    }
}
