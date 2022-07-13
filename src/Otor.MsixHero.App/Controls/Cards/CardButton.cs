using System.Windows;
using System.Windows.Controls.Primitives;

namespace Otor.MsixHero.App.Controls.Cards;

public class CardButton : ButtonBase, ILoadingCard
{
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(CardButton), new PropertyMetadata(null));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(object), typeof(CardButton), new PropertyMetadata(null));

    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(CardButton), new PropertyMetadata(false));

    public static readonly DependencyProperty LoadingContentTemplateProperty = DependencyProperty.Register("LoadingContentTemplate", typeof(DataTemplate), typeof(CardButton), new PropertyMetadata(null));

    public DataTemplate LoadingContentTemplate
    {
        get => (DataTemplate)GetValue(LoadingContentTemplateProperty);
        set => SetValue(LoadingContentTemplateProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}