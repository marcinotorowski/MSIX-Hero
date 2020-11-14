using System.Windows;

namespace Otor.MsixHero.App.Modules.Editors.Signing.PackageSigning.View
{
    internal class DropFileObject : DependencyObject
    {
        public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.RegisterAttached("IsDragging", typeof(bool), typeof(DropFileObject), new PropertyMetadata(false));

        public static bool GetIsDragging(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsDraggingProperty);
        }

        public static void SetIsDragging(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDraggingProperty, value);
        }
    }
}
