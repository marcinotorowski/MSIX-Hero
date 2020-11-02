﻿using System.Windows;
using System.Windows.Controls;
using Fluent;

namespace Otor.MsixHero.Ui.Modules.Main.View
{
    /// <summary>
    /// Interaction logic for BackStageNewView.xaml
    /// </summary>
    public partial class BackStageNewView : UserControl
    {
        public BackStageNewView()
        {
            InitializeComponent();
        }

        private void BackstageButtonClicked(object sender, RoutedEventArgs e)
        {
            PopupService.RaiseDismissPopupEvent(sender, DismissPopupMode.Always);
        }
    }
}