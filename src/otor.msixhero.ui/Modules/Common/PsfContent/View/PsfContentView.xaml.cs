using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.ui.Modules.Common.PsfContent.ViewModel;

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
            //var psfSerializer = new PsfConfigSerializer();
            //this.DataContext = new PsfContentViewModel(psfSerializer.Deserialize(File.ReadAllText(@"E:\temp\msix-psf\fixed-rayeval\config.json")));
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
