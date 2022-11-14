using System.Windows;

namespace Otor.MsixHero.App.Mvvm
{
    internal class DiscreteValidationObject : DependencyObject
    {
        public static readonly DependencyProperty DiscreteValidationTemplateProperty = DependencyProperty.RegisterAttached("DiscreteValidationTemplate", typeof(bool), typeof(DiscreteValidationObject), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        
        public static bool GetDiscreteValidationTemplate(DependencyObject obj)
        {
            return (bool)obj.GetValue(DiscreteValidationTemplateProperty);
        }

        public static void SetDiscreteValidationTemplate(DependencyObject obj, bool value)
        {
            obj.SetValue(DiscreteValidationTemplateProperty, value);
        }
    }
}
