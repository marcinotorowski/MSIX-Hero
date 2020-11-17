using System.Windows;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.View
{
    public class SearchHelper : DependencyObject
    {
        public static readonly DependencyProperty SearchStringProperty = DependencyProperty.RegisterAttached("SearchString", typeof(string), typeof(SearchHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static string GetSearchString(DependencyObject obj)
        {
            return (string)obj.GetValue(SearchStringProperty);
        }

        public static void SetSearchString(DependencyObject obj, string value)
        {
            obj.SetValue(SearchStringProperty, value);
        }
    }
}
