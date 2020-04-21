using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace otor.msixhero.ui.Modules.Common.PsfContent.View
{
    /// <summary>
    /// Interaction logic for PsfContentView.xaml
    /// </summary>
    public partial class PsfContentView
    {
        public PsfContentView()
        {
            InitializeComponent();
        }

        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged)
            {
                return;
            }

            if (e.NewSize.Width < 270 || ((UniformGrid)sender).Children.Count == 1)
            {
                ((UniformGrid) sender).Columns = 1;
            }
            else
            {
                ((UniformGrid) sender).Columns = (int) Math.Floor(e.NewSize.Width / 270.0);
            }
        }
    }
}
