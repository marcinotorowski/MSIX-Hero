using System.Collections.ObjectModel;
using System.Windows;

namespace Otor.MsixHero.App.Controls.PackageExpert.Views.Start
{
    /// <summary>
    /// Interaction logic for Start.xaml
    /// </summary>
    public partial class Start
    {
        public static readonly DependencyProperty ToolsProperty = DependencyProperty.Register("Tools", typeof(ObservableCollection<ToolItem>), typeof(Start), new PropertyMetadata(null));

        public Start()
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
