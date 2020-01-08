using System;
using System.Collections.Generic;
using System.Linq;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Grid
{
    public enum SelectionMode
    {
        SelectAll,
        UnselectAll,
        AddToSelection,
        RemoveFromSelection,
        ReplaceSelection
    }

    [Serializable]
    public class SelectPackages : BaseCommand<List<InstalledPackage>>
    {
        public SelectPackages()
        {
            this.Selection = new List<InstalledPackage>();
            this.SelectionMode = SelectionMode.ReplaceSelection;
        }

        public SelectPackages(List<InstalledPackage> selection, SelectionMode selectionMode = SelectionMode.ReplaceSelection)
        {
            this.Selection = selection;
            this.SelectionMode = selectionMode;
        }

        public SelectPackages(InstalledPackage selection, SelectionMode selectionMode = SelectionMode.ReplaceSelection)
        {
            this.Selection = new List<InstalledPackage> { selection };
            this.SelectionMode = selectionMode;
        }

        public SelectPackages(IEnumerable<InstalledPackage> selection, SelectionMode selectionMode = SelectionMode.ReplaceSelection)
        {
            this.Selection = new List<InstalledPackage>(selection);
            this.SelectionMode = selectionMode;
        }

        public SelectPackages(params InstalledPackage[] selection) : this(selection.ToList())
        {
        }

        public List<InstalledPackage> Selection { get; set; }

        public SelectionMode SelectionMode { get; set; }

        public static SelectPackages CreateSingle(InstalledPackage singleSelection)
        {
            return new SelectPackages(singleSelection);
        }

        public static SelectPackages CreateAddition(InstalledPackage addition)
        {
            return new SelectPackages(new[] { addition }, SelectionMode.AddToSelection);
        }

        public static SelectPackages CreateAddition(params InstalledPackage[] addition)
        {
            return new SelectPackages(addition, SelectionMode.AddToSelection);
        }

        public static SelectPackages CreateSubtraction(InstalledPackage addition)
        {
            return new SelectPackages(new[] { addition }, SelectionMode.RemoveFromSelection);
        }

        public static SelectPackages CreateSubtraction(params InstalledPackage[] addition)
        {
            return new SelectPackages(addition, SelectionMode.RemoveFromSelection);
        }

        public static SelectPackages CreateEmpty()
        {
            return new SelectPackages(new InstalledPackage[0], SelectionMode.UnselectAll);
        }

        public static SelectPackages CreateAll()
        {
            return new SelectPackages(new InstalledPackage[0], SelectionMode.SelectAll);
        }
    }
}
