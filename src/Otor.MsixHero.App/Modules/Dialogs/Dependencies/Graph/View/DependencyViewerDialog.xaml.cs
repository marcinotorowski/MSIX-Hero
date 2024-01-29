// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GraphX.Common.Enums;
using Microsoft.Win32;
using Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.Visuals;

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
                this.ExportAsImageDialog(ImageType.PNG);
            }
            
            public void ExportJpg()
            {
                this.ExportAsImageDialog(ImageType.JPEG);
            }

            public void ExportClipboard()
            {
                throw new NotImplementedException();
            }

            public void Print()
            {
                this.graphArea.PrintDialog();
            }

            private void ExportAsImageDialog(
                ImageType type,
                bool useZoomControlSurface = false,
                double dpi = 96.0,
                int quality = 100)
            {
                string filter;
                switch (type)
                {
                    case ImageType.PNG:
                        filter = "*.png";
                        break;
                    case ImageType.JPEG:
                        filter = "*.jpg";
                        break;
                    case ImageType.BMP:
                        filter = "*.bmp";
                        break;
                    case ImageType.GIF:
                        filter = "*.gif";
                        break;
                    case ImageType.TIFF:
                        filter = "*.tiff";
                        break;
                    default:
                        throw new InvalidDataException("ExportAsImage() -> Unknown output image format specified!");
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = string.Format(MsixHero.App.Resources.Localization.Dialogs_Dependencies_Export_ImageFilter + "|{1}", type.ToString("G").ToUpperInvariant(), filter),
                    Title = string.Format(MsixHero.App.Resources.Localization.Dialogs_Dependencies_Export_Title, filter)
                };

                if (saveFileDialog.ShowDialog() != true)
                {
                    return;
                }

                this.graphArea.ExportAsImage(saveFileDialog.FileName, type, useZoomControlSurface, dpi, quality);
            }
        }
    }
}
