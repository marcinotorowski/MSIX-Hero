using System.Windows;

namespace Otor.MsixHero.App.Helpers
{
    public class ValidatedTabItem : DependencyObject
    {
        public static readonly DependencyProperty IsValidProperty = DependencyProperty.RegisterAttached("IsValid", typeof(bool), typeof(ValidatedTabItem), new PropertyMetadata(true));
        public static readonly DependencyProperty ValidationMessageProperty = DependencyProperty.RegisterAttached("ValidationMessage", typeof(string), typeof(ValidatedTabItem), new PropertyMetadata(null));

        public static bool GetIsValid(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsValidProperty);
        }

        public static void SetIsValid(DependencyObject obj, bool value)
        {
            obj.SetValue(IsValidProperty, value);
        }

        public static string GetValidationMessage(DependencyObject obj)
        {
            return (string)obj.GetValue(ValidationMessageProperty);
        }

        public static void SetValidationMessage(DependencyObject obj, string value)
        {
            obj.SetValue(ValidationMessageProperty, value);
        }
    }
}
