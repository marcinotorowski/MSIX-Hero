using System;
using System.Collections.Generic;
using System.Linq;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;

namespace otor.msixhero.lib.Domain.Commands.Volumes
{
    [Serializable]
    public class SelectVolumes : BaseCommand<List<AppxVolume>>
    {
        public SelectVolumes()
        {
            this.Selection = new List<AppxVolume>();
            this.SelectionMode = SelectionMode.ReplaceSelection;
        }

        public SelectVolumes(List<AppxVolume> selection, SelectionMode selectionMode = SelectionMode.ReplaceSelection)
        {
            this.Selection = selection;
            this.SelectionMode = selectionMode;
        }

        public SelectVolumes(AppxVolume selection, SelectionMode selectionMode = SelectionMode.ReplaceSelection)
        {
            this.Selection = new List<AppxVolume> { selection };
            this.SelectionMode = selectionMode;
        }

        public SelectVolumes(IEnumerable<AppxVolume> selection, SelectionMode selectionMode = SelectionMode.ReplaceSelection)
        {
            this.Selection = new List<AppxVolume>(selection);
            this.SelectionMode = selectionMode;
        }

        public SelectVolumes(params AppxVolume[] selection) : this(selection.ToList())
        {
        }

        public List<AppxVolume> Selection { get; set; }

        public SelectionMode SelectionMode { get; set; }

        public static SelectVolumes CreateSingle(AppxVolume singleSelection)
        {
            return new SelectVolumes(singleSelection);
        }

        public static SelectVolumes CreateAddition(AppxVolume addition)
        {
            return new SelectVolumes(new[] { addition }, SelectionMode.AddToSelection);
        }

        public static SelectVolumes CreateAddition(params AppxVolume[] addition)
        {
            return new SelectVolumes(addition, SelectionMode.AddToSelection);
        }

        public static SelectVolumes CreateSubtraction(AppxVolume addition)
        {
            return new SelectVolumes(new[] { addition }, SelectionMode.RemoveFromSelection);
        }

        public static SelectVolumes CreateSubtraction(params AppxVolume[] addition)
        {
            return new SelectVolumes(addition, SelectionMode.RemoveFromSelection);
        }

        public static SelectVolumes CreateEmpty()
        {
            return new SelectVolumes(new AppxVolume[0], SelectionMode.UnselectAll);
        }

        public static SelectVolumes CreateAll()
        {
            return new SelectVolumes(new AppxVolume[0], SelectionMode.SelectAll);
        }
    }
}
