using System.Collections.Generic;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedPackageContainer.Navigation
{
    public class NavigationRequest
    {
        public NavigationRequest()
        {
            this.Type = EditingType.New;
        }

        public NavigationRequest(IDialogParameters parameters)
        {
            this.Type = EditingType.New;

            foreach (var parameter in parameters.Keys)
            {
                switch (parameter)
                {
                    case nameof(ContainerName):
                        this.ContainerName = parameters.GetValue<string>(parameter);
                        break;
                    case nameof(Type):
                        this.Type = parameters.GetValue<EditingType>(parameter);
                        break;
                    case nameof(ConflictResolution):
                        this.ConflictResolution = parameters.GetValue<ContainerConflictResolution>(parameter);
                        break;
                    case nameof(ForceApplicationShutdown):
                        this.ForceApplicationShutdown = parameters.GetValue<bool>(parameter);
                        break;
                    default:
                        if (parameter.StartsWith(nameof(Items) + "_"))
                        {
                            if (this.Items == null)
                            {
                                this.Items = new List<string>();
                            }

                            this.Items.Add(parameters.GetValue<string>(parameter));
                        }

                        break;
                }
            }
        }

        public IDialogParameters ToDialogParameters()
        {
            var navParam = new DialogParameters();
            
            if (!string.IsNullOrEmpty(this.ContainerName))
            {
                navParam.Add(nameof(ContainerName), this.ContainerName);
            }

            if (this.Items != null)
            {
                var index = 1;
                foreach (var item in this.Items)
                {
                    navParam.Add(nameof(Items) + "_" + index, item);
                    index++;
                }
            }

            if (this.ConflictResolution.HasValue)
            {
                navParam.Add(nameof(ConflictResolution), this.ConflictResolution.Value);
            }

            if (this.ForceApplicationShutdown.HasValue)
            {
                navParam.Add(nameof(ForceApplicationShutdown), this.ForceApplicationShutdown.Value);
            }

            navParam.Add(nameof(Type), this.Type);

            return navParam;
        }

        public EditingType Type { get; set; }

        public string ContainerName { get; set; }

        public IList<string> Items { get; set; }

        public ContainerConflictResolution? ConflictResolution { get; set; }

        public bool? ForceApplicationShutdown { get; set; }
        
        public enum EditingType
        {
            New,
            EditRunning,
            OpenXml
        }
    }
}
