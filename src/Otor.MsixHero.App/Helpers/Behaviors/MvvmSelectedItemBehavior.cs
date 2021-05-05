using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items.Content;

namespace Otor.MsixHero.App.Helpers.Behaviors
{
    // ReSharper disable once IdentifierTypo
    public class MvvmSelectedItemBehavior : Behavior<TreeView>
    {
        #region SelectedItem Property

        public object SelectedItem
        {
            get => this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(MvvmSelectedItemBehavior), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is TreeViewItem tvi)
            {
                tvi.SetValue(TreeViewItem.IsSelectedProperty, true);
            }
            else if (e.NewValue is ISelectableItem item)
            {
                item.IsSelected = true;
            }
            else if (e.NewValue != null)
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectedItemChanged -= this.OnTreeViewSelectedItemChanged;
            this.AssociatedObject.SelectedItemChanged += this.OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.SelectedItemChanged -= this.OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.SelectedItem = e.NewValue;
        }
    }
}
