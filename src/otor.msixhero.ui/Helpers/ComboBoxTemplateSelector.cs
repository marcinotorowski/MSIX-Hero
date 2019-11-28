using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace otor.msixhero.ui.Helpers
{
    public class ComboBoxTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SelectedTemplate { get; set; }

        public DataTemplate ListTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var presenter = (ContentPresenter)container;

            if (presenter.TemplatedParent is ComboBox)
            {
                return this.SelectedTemplate ?? this.ListTemplate;
            }

            return this.ListTemplate;
        }
    }
}
