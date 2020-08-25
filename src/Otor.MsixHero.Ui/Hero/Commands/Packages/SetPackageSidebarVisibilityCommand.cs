using System;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands.Packages
{
    [Serializable]
    public class SetPackageSidebarVisibilityCommand : UiCommand
    {
        public SetPackageSidebarVisibilityCommand() : this(true)
        {
        }

        public SetPackageSidebarVisibilityCommand(bool isVisible)
        {
            IsVisible = isVisible;
        }

        public bool IsVisible { get; set; }
    }
}
