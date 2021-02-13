using System.Windows;
using System.Windows.Controls;

namespace Otor.MsixHero.App.Controls.TreeListView
{
    /// <remarks>Inspired by: https://www.codeproject.com/script/Articles/ViewDownloads.aspx?aid=24973</remarks>
    public class TreeListViewControl : TreeView
    {
        public static readonly DependencyProperty AllowsColumnReorderProperty = DependencyProperty.Register("AllowsColumnReorder", typeof(bool), typeof(TreeListViewControl), new UIPropertyMetadata(null));
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(GridViewColumnCollection), typeof(TreeListViewControl), new UIPropertyMetadata(null));
        
        static TreeListViewControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListViewControl), new FrameworkPropertyMetadata(typeof(TreeListViewControl)));
        }
        
        public TreeListViewControl()
        {
            Columns = new GridViewColumnCollection();
        }
        
        public GridViewColumnCollection Columns
        {
            get => (GridViewColumnCollection)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }
        
        public bool AllowsColumnReorder
        {
            get => (bool)GetValue(AllowsColumnReorderProperty);
            set => SetValue(AllowsColumnReorderProperty, value);
        }
    }
}