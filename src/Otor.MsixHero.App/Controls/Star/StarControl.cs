using System.Windows;
using System.Windows.Controls;

namespace Otor.MsixHero.App.Controls.Star
{
    public class StarControl : CheckBox
    {
        public static readonly DependencyProperty ToolTipStarredProperty = DependencyProperty.Register(nameof(ToolTipStarred), typeof(object), typeof(StarControl), new PropertyMetadata(null));
        
        public static readonly DependencyProperty ToolTipNotStarredProperty = DependencyProperty.Register(nameof(ToolTipNotStarred), typeof(object), typeof(StarControl), new PropertyMetadata(null));

        public object ToolTipStarred
        {
            get => GetValue(ToolTipStarredProperty);
            set => SetValue(ToolTipStarredProperty, value);
        }

        public object ToolTipNotStarred
        {
            get => GetValue(ToolTipNotStarredProperty);
            set => SetValue(ToolTipNotStarredProperty, value);
        }
    }
}
