using System.Windows;
using System.Windows.Controls.Primitives;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.View.Overview.Actions
{
    public partial class ActionsView
    {
        public ActionsView()
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
    }
}
