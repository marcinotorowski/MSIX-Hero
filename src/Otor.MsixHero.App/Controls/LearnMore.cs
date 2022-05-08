using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Otor.MsixHero.App.Services;
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.App.Controls
{
    [TemplatePart(Name = "PART_Hyperlink", Type = typeof(Hyperlink))]
    public class LearnMore : Control
    {
        static LearnMore()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LearnMore), new FrameworkPropertyMetadata(typeof(LearnMore)));
        }
        
        public static readonly DependencyProperty TopicProperty = DependencyProperty.Register("Topic", typeof(string), typeof(LearnMore), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty LinkProperty = DependencyProperty.Register("Link", typeof(string), typeof(LearnMore), new PropertyMetadata(default(string)));

        public string Topic
        {
            get => (string) GetValue(TopicProperty);
            set => SetValue(TopicProperty, value);
        }

        public string Link
        {
            get => (string) GetValue(LinkProperty);
            set => SetValue(LinkProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var hyperlink = (Hyperlink)this.GetTemplateChild("PART_Hyperlink");
            if (hyperlink == null)
            {
                return;
            }
            
            hyperlink.Click += HyperlinkOnClick;
        }

        private void HyperlinkOnClick(object sender, RoutedEventArgs e)
        {
            var url = this.Link;

            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var parsed))
            {
                return;
            }

            if (!parsed.IsAbsoluteUri)
            {
                url = "https://msixhero.net/redirect/" + url;
            }

            ExceptionGuard.Guard(() =>
                {
                    var psi = new ProcessStartInfo(url)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                },
                new InteractionService());
        }
    }
}
