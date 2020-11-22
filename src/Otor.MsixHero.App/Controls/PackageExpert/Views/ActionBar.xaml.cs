using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
            this.PopupMore.CustomPopupPlacementCallback = this.CustomPopupPlacementCallback;
        }

        private CustomPopupPlacement[] CustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            return new[]
            {
                new CustomPopupPlacement(new Point(offset.X - popupSize.Width + targetSize.Width, offset.Y + targetSize.Height), PopupPrimaryAxis.Vertical) 
            };
        }

        public ObservableCollection<ToolItem> Tools 
        {
            get => (ObservableCollection<ToolItem>)GetValue(ToolsProperty);
            set => SetValue(ToolsProperty, value);
        }
    }
}
