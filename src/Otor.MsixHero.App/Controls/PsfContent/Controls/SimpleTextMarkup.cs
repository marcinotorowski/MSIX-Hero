using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Otor.MsixHero.App.Controls.PsfContent.Controls
{
    internal class SimpleTextMarkup : DependencyObject
    {
        public static readonly DependencyProperty MarkupProperty = DependencyProperty.RegisterAttached("Markup", typeof(string), typeof(SimpleTextMarkup), new PropertyMetadata(null, OnMarkupChanged));

        private static void OnMarkupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (TextBlock)d;
            InsertMarkup(target, (string)e.NewValue);
        }

        public static string GetMarkup(DependencyObject obj)
        {
            return (string)obj.GetValue(MarkupProperty);
        }

        public static void SetMarkup(DependencyObject obj, string value)
        {
            obj.SetValue(MarkupProperty, value);
        }
        
        public static void InsertMarkup(TextBlock control, string text)
        { 
            InsertMarkup(control.Inlines, text);
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
