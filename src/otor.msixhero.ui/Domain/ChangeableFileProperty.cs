using System;
using System.IO;
using System.Windows.Input;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.ui.Commands.RoutedCommand;

namespace otor.msixhero.ui.Domain
{
    public class ChangeableFileProperty : ValidatedChangeableProperty<string>
    {
        private static Func<string, string> validatePath;
        private static Func<string, string> validatePathAndPresence;

        private readonly IInteractionService interactionService;
        private ICommand browse;

        public ChangeableFileProperty(IInteractionService interactionService, string initialFile = default) : base(initialFile)
        {
            this.interactionService = interactionService;
        }

        public string Filter { get; set; }

        public bool OpenForSaving { get; set; }

        public static Func<string, string> ValidatePath
        {
            get
            {
                return validatePath ??= value =>
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        return "The file path may not be empty.";
                    }

                    if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                    {
                        return "The file path is invalid.";
                    }

                    return null;
                };
            }
        }

        public static Func<string, string> ValidatePathAndPresence
        {
            get
            {
                return validatePathAndPresence ??= value =>
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        return "The file path may not be empty.";
                    }

                    if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                    {
                        return "The file path is invalid.";
                    }


                    if (!File.Exists(value))
                    {
                        return "The file does not exist.";
                    }

                    return null;
                };
            }
        }

        public ICommand Browse
        {
            get
            {
                return this.browse ??= new DelegateCommand(param =>
                {
                    if (this.OpenForSaving)
                    {
                        if (string.IsNullOrEmpty(this.CurrentValue))
                        {
                            if (this.interactionService.SaveFile(this.Filter, out var newValue))
                            {
                                this.CurrentValue = newValue;
                            }
                        }
                        else
                        {
                            if (this.interactionService.SaveFile(this.CurrentValue, this.Filter, out var newValue))
                            {
                                this.CurrentValue = newValue;
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(this.CurrentValue))
                        {
                            if (this.interactionService.SelectFile(this.Filter, out var newValue))
                            {
                                this.CurrentValue = newValue;
                            }
                        }
                        else
                        {
                            if (this.interactionService.SelectFile(this.CurrentValue, this.Filter, out var newValue))
                            {
                                this.CurrentValue = newValue;
                            }
                        }
                    }
                });
            }
        }
    }
}