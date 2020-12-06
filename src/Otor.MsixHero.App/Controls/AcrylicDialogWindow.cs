using System.Windows.Media;
using Prism.Services.Dialogs;
using SourceChord.FluentWPF;

namespace Otor.MsixHero.App.Controls
{
    public class AcrylicDialogWindow : AcrylicWindow, IDialogWindow
    {
        public AcrylicDialogWindow()
        {
            this.ExtendViewIntoTitleBar = true;
            // ReSharper disable once PossibleNullReferenceException
            this.TintColor = (Color)ColorConverter.ConvertFromString("#0173C7");
            this.TintOpacity = 0.2;
        }

        public IDialogResult Result { get; set; }
    }

}
