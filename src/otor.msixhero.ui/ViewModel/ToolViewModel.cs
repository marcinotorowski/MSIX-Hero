namespace otor.msixhero.ui.ViewModel
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
