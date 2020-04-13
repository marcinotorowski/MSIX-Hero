using System.IO;
using System.Windows.Controls;
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
            var psfSerializer = new PsfConfigSerializer();
            this.DataContext = new PsfContentViewModel(psfSerializer.Deserialize(File.ReadAllText(@"E:\temp\msix-psf\fixed-rayeval\config.json")));
        }
    }
}
