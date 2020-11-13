using System.Windows.Controls;

namespace Otor.MsixHero.App.Controls
{
    public class EmptyControl : Control
    {
        private EmptyControl()
        {
        }

        public static EmptyControl Instance { get; } = new EmptyControl();
    }
}
