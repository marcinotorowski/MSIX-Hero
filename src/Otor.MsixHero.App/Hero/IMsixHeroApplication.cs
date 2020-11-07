using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;

namespace Otor.MsixHero.App.Hero
{
    public interface IMsixHeroApplication
    {
        IMsixHeroCommandExecutor CommandExecutor { get; }

        IEventAggregator EventAggregator { get; }

        MsixHeroState ApplicationState { get; }

        IConfigurationService ConfigurationService { get; }
    }
}
