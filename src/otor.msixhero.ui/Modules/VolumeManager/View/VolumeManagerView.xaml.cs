using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Otor.MsixHero.Lib.BusinessLayer.State;
using Prism.Events;

namespace Otor.MsixHero.Ui.Modules.VolumeManager.View
{
    /// <summary>
    /// Interaction logic for Volume Manager.
    /// </summary>
    public partial class VolumeManagerView
    {
        public VolumeManagerView(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            FocusManager.SetFocusedElement(this, this.ListBox);
            this.IsVisibleChanged += OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool) e.NewValue)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                FocusManager.SetFocusedElement(Application.Current.MainWindow, this.ListBox);
                FocusManager.SetFocusedElement(this, this.ListBox);
                this.ListBox.Focus();
                Keyboard.Focus(this.ListBox);
            }, DispatcherPriority.ApplicationIdle);
        }
        
        private void ClearSearchField(object sender, RoutedEventArgs e)
        {
            this.SearchBox.Text = string.Empty;
            this.SearchBox.Focus();
            FocusManager.SetFocusedElement(this, this.SearchBox);
            Keyboard.Focus(this.SearchBox);
        }

        private void SearchCommandBindingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Keyboard.Focus(this.SearchBox);
            FocusManager.SetFocusedElement(this, this.SearchBox);
            this.SearchBox.Focus();
        }
    }
}
