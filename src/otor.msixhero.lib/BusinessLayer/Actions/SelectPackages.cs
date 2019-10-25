using System.Collections.Generic;

namespace otor.msixhero.lib.BusinessLayer.Actions
{
    public enum SelectionMode
    {
        SelectAll,
        UnselectAll,
        AddToSelection,
        RemoveFromSelection,
        ReplaceSelection
    }

    public class SelectPackages : BaseAction
    {
        public SelectPackages(IReadOnlyCollection<Package> selection, SelectionMode selectionMode = SelectionMode.ReplaceSelection)
        {
            Selection = selection;
            SelectionMode = selectionMode;
        }
        
        public SelectPackages(params Package[] selection) : this(selection, SelectionMode.ReplaceSelection)
        {
        }

        public IReadOnlyCollection<Package> Selection { get; set; }

        public SelectionMode SelectionMode { get; set; }

        public static SelectPackages CreateSingle(Package singleSelection)
        {
            return new SelectPackages(singleSelection);
        }

        public static SelectPackages CreateAddition(Package addition)
        {
            return new SelectPackages(new[] { addition }, SelectionMode.AddToSelection);
        }

        public static SelectPackages CreateAddition(params Package[] addition)
        {
            return new SelectPackages(addition, SelectionMode.AddToSelection);
        }

        public static SelectPackages CreateSubtraction(Package addition)
        {
            return new SelectPackages(new[] { addition }, SelectionMode.RemoveFromSelection);
        }

        public static SelectPackages CreateSubtraction(params Package[] addition)
        {
            return new SelectPackages(addition, SelectionMode.RemoveFromSelection);
        }

        public static SelectPackages CreateEmpty()
        {
            return new SelectPackages(new Package[0], SelectionMode.UnselectAll);
        }

        public static SelectPackages CreateAll()
        {
            return new SelectPackages(new Package[0], SelectionMode.SelectAll);
        }
    }
}
