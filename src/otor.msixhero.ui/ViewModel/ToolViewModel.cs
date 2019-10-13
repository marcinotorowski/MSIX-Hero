using System;
using System.Collections.Generic;
using System.Text;

namespace MSI_Hero.ViewModel
{
    public class ToolViewModel : NotifyPropertyChanged
    {
        public ToolViewModel(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
