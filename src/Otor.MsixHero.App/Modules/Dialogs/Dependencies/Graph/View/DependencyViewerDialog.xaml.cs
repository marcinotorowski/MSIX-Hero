// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System.Windows;
using System.Windows.Input;
using GraphX.Common.Enums;
using Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.Model;
using Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.ViewModel;

namespace Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.View
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
