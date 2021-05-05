using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Xaml.Behaviors;
using Notifications.Wpf.Core;
using Otor.MsixHero.App.Services;
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.App.Helpers.Behaviors
{
    public class HyperlinkClickBehavior : Behavior<Hyperlink>
    {
        public static readonly DependencyProperty UrlProperty = DependencyProperty.Register("Url", typeof(string), typeof(HyperlinkClickBehavior), new PropertyMetadata(default(string)));

        public string Url
        {
            get => (string) GetValue(UrlProperty);
            set => SetValue(UrlProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Click += AssociatedObjectOnClick;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.Click -= this.AssociatedObjectOnClick;
            base.OnDetaching();
        }

        private void AssociatedObjectOnClick(object sender, RoutedEventArgs e)
        {
            ExceptionGuard.Guard(() =>
                {
                    var psi = new ProcessStartInfo(this.Url)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(psi);
            },
            new InteractionService(new NotificationManager()));
        }
    }
}
