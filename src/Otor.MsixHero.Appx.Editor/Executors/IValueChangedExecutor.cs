using System;
using Otor.MsixHero.Appx.Editor.Executors.Concrete;

namespace Otor.MsixHero.Appx.Editor.Executors
{
    public interface IValueChangedExecutor
    {
        event EventHandler<CommandValueChanged> ValueChanged;
    }
}
