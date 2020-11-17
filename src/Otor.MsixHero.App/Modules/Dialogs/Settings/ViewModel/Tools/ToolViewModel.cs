using System.Windows.Media;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel.Tools
{
    public class ToolViewModel : ChangeableContainer
    {
        private readonly ToolListConfiguration model;

        public ToolViewModel(IInteractionService interactionService, ToolListConfiguration model)
        {
            this.model = model;
            
            this.AddChildren(
                this.Path = new ChangeableFileProperty("Command path", interactionService, model.Path, ValidatePath),
                this.Name = new ValidatedChangeableProperty<string>("Command name", model.Name, ValidateName),
                this.Icon = new ChangeableFileProperty("Command icon", interactionService, model.Icon),
                this.Arguments = new ChangeableProperty<string>(model.Arguments),
                this.AsAdmin = new ChangeableProperty<bool>(model.AsAdmin)
            );

            this.Path.ValueChanged += (sender, args) => { this.OnPropertyChanged(nameof(Image)); };
            this.Icon.ValueChanged += this.IconOnValueChanged;
        }

        public ChangeableFileProperty Path { get; }

        public ValidatedChangeableProperty<string> Name { get; }
        
        public ChangeableProperty<string> Arguments { get; }

        public ChangeableFileProperty Icon { get; }

        public ChangeableProperty<bool> AsAdmin { get; }

        public bool HasIcon => !string.IsNullOrEmpty(this.Icon.CurrentValue);

        public ImageSource Image => ShellIcon.GetIconFor(string.IsNullOrEmpty(this.Icon.CurrentValue) ? this.Path.CurrentValue : this.Icon.CurrentValue);

        public static implicit operator ToolListConfiguration(ToolViewModel viewModel)
        {
            return viewModel.model;
        }

        public override void Commit()
        {
            if (this.Path.IsTouched)
            {
                this.model.Path = this.Path.CurrentValue;
            }

            if (this.Name.IsTouched)
            {
                this.model.Name = this.Name.CurrentValue;
            }

            if (this.Icon.IsTouched)
            {
                this.model.Icon = this.Icon.CurrentValue;
            }

            if (this.AsAdmin.IsTouched)
            {
                this.model.AsAdmin = this.AsAdmin.CurrentValue;
            }

            if (this.Arguments.IsTouched)
            {
                this.model.Arguments = this.Arguments.CurrentValue;
            }

            base.Commit();
        }

        private static string ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "The path to a tool may not be empty.";
            }

            return null;
        }

        private static string ValidateName(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return null;
            }

            return "The name may not be empty.";
        }

        private void IconOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(HasIcon));
            this.OnPropertyChanged(nameof(Image));
        }
    }
}
