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
        
        private static readonly DependencyPropertyKey ResolvedUrlPropertyKey = DependencyProperty.RegisterAttachedReadOnly("ResolvedUrl", typeof(string), typeof(LearnMore), new PropertyMetadata(null));
        public static readonly DependencyProperty ResolvedUrlProperty = ResolvedUrlPropertyKey.DependencyProperty;

        public static readonly DependencyProperty TopicProperty = DependencyProperty.Register("Topic", typeof(string), typeof(LearnMore), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty LinkProperty = DependencyProperty.Register("Link", typeof(string), typeof(LearnMore), new PropertyMetadata(default(string), OnLinkChanged));

        public string ResolvedUrl
        {
            get => (string)GetValue(ResolvedUrlProperty);
            private set => SetValue(ResolvedUrlPropertyKey, value);
        }

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

        private static void OnLinkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string resolvedUrl;
            var uri = e.NewValue as string;
            if (uri == null)
            {
                resolvedUrl = null;
            }
            else
            {
                if (!Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out var parsed))
                {
                    resolvedUrl = "https://msixhero.net/redirect/" + uri;
                }
                else if (!parsed.IsAbsoluteUri)
                {
                    resolvedUrl = "https://msixhero.net/redirect/" + uri;
                }
                else
                {
                    resolvedUrl = uri;
                }
            }

            ((LearnMore)d).ResolvedUrl = resolvedUrl;
        }

        private void HyperlinkOnClick(object sender, RoutedEventArgs e)
        {
            var url = this.ResolvedUrl;

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
                    // ReSharper disable once AssignNullToNotNullAttribute
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
