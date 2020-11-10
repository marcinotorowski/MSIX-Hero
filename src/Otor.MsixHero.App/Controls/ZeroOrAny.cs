using System.Windows;
using System.Windows.Controls;

namespace Otor.MsixHero.App.Controls
{
    public class ZeroOrAny : Control
    {
        public static readonly DependencyProperty NoItemsTemplateProperty =  DependencyProperty.Register("NoItemsTemplate", typeof(DataTemplate), typeof(ZeroOrAny), new PropertyMetadata(null));
        public static readonly DependencyProperty AnyItemTemplateProperty =  DependencyProperty.Register("AnyItemTemplate", typeof(DataTemplate), typeof(ZeroOrAny), new PropertyMetadata(null));
        public static readonly DependencyProperty OneItemTemplateProperty =  DependencyProperty.Register("OneItemTemplate", typeof(DataTemplate), typeof(ZeroOrAny), new PropertyMetadata(null));
        public static readonly DependencyProperty ManyItemsTemplateProperty =  DependencyProperty.Register("ManyItemsTemplate", typeof(DataTemplate), typeof(ZeroOrAny), new PropertyMetadata(null));
        public static readonly DependencyProperty ItemsCountProperty =  DependencyProperty.Register("ItemsCount", typeof(int), typeof(ZeroOrAny), new PropertyMetadata(0, OnItemsChanged));
        public static readonly DependencyProperty ActualTemplateProperty = DependencyProperty.Register("ActualTemplate", typeof(DataTemplate), typeof(ZeroOrAny), new PropertyMetadata(null));

        public DataTemplate ActualTemplate
        {
            get => (DataTemplate)GetValue(ActualTemplateProperty);
            set => this.SetValue(ActualTemplateProperty, value);
        }

        public int ItemsCount
        {
            get => (int)GetValue(ItemsCountProperty);
            set => this.SetValue(ItemsCountProperty, value);
        }

        public DataTemplate NoItemsTemplate
        {
            get => (DataTemplate)this.GetValue(NoItemsTemplateProperty);
            set => this.SetValue(NoItemsTemplateProperty, value);
        }


        public DataTemplate OneItemTemplate
        {
            get => (DataTemplate)this.GetValue(OneItemTemplateProperty);
            set => this.SetValue(OneItemTemplateProperty, value);
        }

        public DataTemplate ManyItemsTemplate
        {
            get => (DataTemplate)this.GetValue(ManyItemsTemplateProperty);
            set => this.SetValue(ManyItemsTemplateProperty, value);
        }

        public DataTemplate AnyItemTemplate
        {
            get => (DataTemplate)this.GetValue(AnyItemTemplateProperty);
            set => this.SetValue(AnyItemTemplateProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.SetTemplate();
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that = (ZeroOrAny)d;
            that.SetTemplate();
        }

        private void SetTemplate()
        {
            var colItemsCount = this.ItemsCount;

            if (colItemsCount == 0)
            {
                this.ActualTemplate = this.NoItemsTemplate;
            }
            else if (colItemsCount == 1)
            {
                if (this.OneItemTemplate != null)
                {
                    this.ActualTemplate = this.OneItemTemplate;
                }
                else
                {
                    this.ActualTemplate = this.AnyItemTemplate;
                }
            }
            else
            {
                if (this.ManyItemsTemplate != null)
                {
                    this.ActualTemplate = this.ManyItemsTemplate;
                }
                else
                {
                    this.ActualTemplate = this.AnyItemTemplate;
                }
            }
        }
    }
}
