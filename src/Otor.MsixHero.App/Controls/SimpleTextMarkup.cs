using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Otor.MsixHero.App.Controls
{
    internal class SimpleTextMarkup : DependencyObject
    {
        public static readonly DependencyProperty MarkupProperty = DependencyProperty.RegisterAttached("Markup", typeof(string), typeof(SimpleTextMarkup), new PropertyMetadata(null, OnMarkupChanged));

        private static void OnMarkupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                InsertMarkup(textBlock.Inlines, (string)e.NewValue);
            }
            else if (d is Span hyperLink)
            {
                InsertMarkup(hyperLink.Inlines, (string)e.NewValue);
            }
            else
            {
                throw new NotSupportedException($"Target element {d.GetType()} is not supported by this property.");
            }
        }

        public static string GetMarkup(DependencyObject obj)
        {
            return (string)obj.GetValue(MarkupProperty);
        }

        public static void SetMarkup(DependencyObject obj, string value)
        {
            obj.SetValue(MarkupProperty, value);
        }

        public static void InsertMarkup(InlineCollection collection, string text)
        {
            collection.Clear();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var index = 0;
            var isBold = false;

            while (index < text.Length)
            {
                var nextStars = text.IndexOf("**", index, StringComparison.Ordinal);
                var currentText = nextStars == -1 ? text.Substring(index) : text.Substring(index, nextStars - index);

                if (isBold)
                {
                    collection.Add(new Bold(new Run(currentText)));
                }
                else
                {
                    collection.Add(currentText);
                }

                isBold = !isBold;
                if (nextStars == -1)
                {
                    break;
                }

                index = nextStars + "**".Length;
            }
        }
    }
}
