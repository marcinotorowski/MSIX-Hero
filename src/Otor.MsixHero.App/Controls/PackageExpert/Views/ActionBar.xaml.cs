using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Otor.MsixHero.App.Controls.PackageExpert.Views
{
    /// <summary>
    /// Interaction logic for ActionBar.
    /// </summary>
    public partial class ActionBar
    {
        public static readonly DependencyProperty ToolsProperty = DependencyProperty.Register("Tools", typeof(ObservableCollection<ToolItem>), typeof(ActionBar), new PropertyMetadata(null));

        public ActionBar()
        {
            InitializeComponent();
        }

        public ObservableCollection<ToolItem> Tools 
        {
            get => (ObservableCollection<ToolItem>)GetValue(ToolsProperty);
            set => SetValue(ToolsProperty, value);
        }
    }
}
