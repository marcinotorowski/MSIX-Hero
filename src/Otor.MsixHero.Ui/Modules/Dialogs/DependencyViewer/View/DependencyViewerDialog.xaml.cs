using System.Windows;
using System.Windows.Input;
using GraphX.Common.Enums;
using Otor.MsixHero.Ui.Modules.Dialogs.DependencyViewer.Model;
using Otor.MsixHero.Ui.Modules.Dialogs.DependencyViewer.ViewModel;

namespace Otor.MsixHero.Ui.Modules.Dialogs.DependencyViewer.View
{
    /// <summary>
    /// Interaction logic for Dependency Viewer Dialog.
    /// </summary>
    public partial class DependencyViewerDialog
    {
        private readonly MyExporterHandler exportHandler;

        public DependencyViewerDialog()
        {
            InitializeComponent();
            this.exportHandler = new MyExporterHandler(this.GraphArea);
            if (this.DataContext is DependencyViewerViewModel dvm)
            {
                dvm.LogicLoaded += this.OnLogicLoaded;
                dvm.ExporterHandler = this.exportHandler;
            }

            this.DataContextChanged += this.OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is DependencyViewerViewModel old)
            {
                old.LogicLoaded -= this.OnLogicLoaded;
                if (old.ExporterHandler == this.exportHandler)
                {
                    old.ExporterHandler = null;
                }
            }

            if (e.NewValue is DependencyViewerViewModel @new)
            {
                @new.LogicLoaded -= this.OnLogicLoaded;
                @new.LogicLoaded += this.OnLogicLoaded;
                @new.ExporterHandler = this.exportHandler;
            }
        }

        private void OnLogicLoaded(object sender, DependencyLogicCore newValue)
        {
            this.GraphArea.LogicCore = newValue;
            this.GraphArea.GenerateGraph();
            this.ZoomControl.ZoomToFill();
        }

        private void HeaderClicked(object sender, MouseButtonEventArgs e)
        {
            ((DependencyViewerViewModel) this.DataContext).Path.Browse.Execute(null);
            ((DependencyViewerViewModel) this.DataContext).Analyze.Execute(null);
        }

        public class MyExporterHandler : DependencyViewerViewModel.IExporterHandler
        {
            private readonly DependencyGraphArea graphArea;

            public MyExporterHandler(DependencyGraphArea graphArea)
            {
                this.graphArea = graphArea;
            }

            public void ExportPng()
            {
                this.graphArea.ExportAsImageDialog(ImageType.PNG);
            }

            public void ExportJpg()
            {
                this.graphArea.ExportAsImageDialog(ImageType.JPEG);
            }

            public void Print()
            {
                this.graphArea.PrintDialog();
            }
        }
    }
}
