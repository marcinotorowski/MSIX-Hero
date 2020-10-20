using GraphX.Common.Enums;
using GraphX.Common.Models;

namespace Otor.MsixHero.Ui.Modules.Dialogs.DependencyViewer.Model
{
    public class DependencyVertex : VertexBase
    {
        /// <summary>
        /// Some string property for example purposes
        /// </summary>
        public string Text { get; set; }

        public VertexShape Shape { get; set; } = VertexShape.Rectangle;

        public override string ToString()
        {
            return Text;
        }
    }
}