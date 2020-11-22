using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Otor.MsixHero.App.Helpers
{
    public class ButtonClosePopup : Behavior<ButtonBase>
    {
        public static readonly DependencyProperty PopupOwnerProperty = DependencyProperty.RegisterAttached("PopupOwner", typeof(Popup), typeof(ButtonClosePopup), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.PreviewMouseUp += ButtonMouseUp;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewMouseUp -= ButtonMouseUp;
            base.OnDetaching();
        }
        
        public static Popup GetPopupOwner(DependencyObject obj)
        {
            return (Popup)obj.GetValue(PopupOwnerProperty);
        }

        public static void SetPopupOwner(DependencyObject obj, Popup value)
        {
            obj.SetValue(PopupOwnerProperty, value);
        }
        
        private static void ButtonMouseUp(object sender, MouseButtonEventArgs e)
        {
            var button = (ButtonBase) sender;
            var contentWithPopup = button.GetVisualParent<DependencyObject>(d => GetPopupOwner(d) != null);
            if (contentWithPopup != null)
            {
                GetPopupOwner(contentWithPopup).IsOpen = false;
            }
        }
    }
}
