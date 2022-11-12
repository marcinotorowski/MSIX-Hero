using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel.Tabs.Signing;
using System.Windows.Input;
using System.Windows.Threading;
using System.Collections.Specialized;
using System.Linq;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.View.Tabs.Signing
{
    public partial class SigningSettingsTab
    {
        public SigningSettingsTab()
        {
            InitializeComponent();
            this.DataContextChanged += this.OnDataContextChanged;
            this.PreviewMouseDown += this.OnPreviewMouseDown;

            if (this.DataContext is SigningSettingsTabViewModel dataContext)
            {
                dataContext.Profiles.CollectionChanged += this.ProfilesOnCollectionChanged;
            }
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dataContext = (SigningSettingsTabViewModel)this.DataContext;
            var editing = dataContext.SelectedProfile;

            if (editing?.IsEditing != true)
            {
                return;
            }

            var container = (ListBoxItem)this.ListBox.ItemContainerGenerator.ContainerFromItem(editing);
            if (!container.IsMouseOver)
            {
                editing.IsEditing = false;
            }
        }

        private void ListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = (ListBox)sender;

            for (var i = 0; i < listBox.Items.Count; i++)
            {
                var container = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(i);
                var viewModel = (SignProfileViewModel)container.DataContext;
                if (container.IsMouseOver)
                {
                    viewModel.IsEditing = true;
                }
                else
                {
                    viewModel.IsEditing = false;
                }
            }
        }

        private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox)sender;
            var viewModel = (SignProfileViewModel)textBox.DataContext;

            switch (e.Key)
            {
                case Key.Enter:
                    viewModel.DisplayName.CurrentValue = textBox.Text;
                    viewModel.IsEditing = false;
                    break;
                case Key.Escape:
                    viewModel.IsEditing = false;
                    break;
            }
        }

        private void TextBox_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                return;
            }

            var textBox = (TextBox)sender;
            var viewModel = (SignProfileViewModel)textBox.DataContext;
            if (viewModel == null)
            {
                return;
            }

            textBox.Text = viewModel.DisplayName.CurrentValue;

            Dispatcher.BeginInvoke(() =>
            {
                var window = Window.GetWindow(textBox);
                if (window == null)
                {
                    return;
                }

                textBox.Focus();
                Keyboard.Focus(textBox);
                FocusManager.SetFocusedElement(window, textBox);
                textBox.SelectAll();
            }, DispatcherPriority.SystemIdle);
        }

        private void ListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F2)
            {
                return;
            }

            var dataContext = (SigningSettingsTabViewModel)this.DataContext;
            var editing = dataContext.SelectedProfile;

            if (editing?.IsEditing != false)
            {
                return;
            }

            editing.IsEditing = true;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is SigningSettingsTabViewModel dataContext)
            {
                dataContext.Profiles.CollectionChanged += this.ProfilesOnCollectionChanged;
            }
        }

        private void ProfilesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count != 1)
            {
                return;
            }

            var addedItem = (SignProfileViewModel)e.NewItems[0];
            if (addedItem == null)
            {
                return;
            }

            var dataContext = (SigningSettingsTabViewModel)this.DataContext;
            if (!dataContext.Profiles.Any(a => a.IsEditing))
            {
                Dispatcher.BeginInvoke(() => addedItem.IsEditing = true, DispatcherPriority.SystemIdle);
            }
        }
    }
}
