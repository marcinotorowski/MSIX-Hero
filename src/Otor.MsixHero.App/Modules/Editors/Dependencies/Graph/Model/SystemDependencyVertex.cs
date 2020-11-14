using System;
using GraphX.Common.Enums;

namespace Otor.MsixHero.App.Modules.Editors.Dependencies.Graph.Model
{
    public class SystemDependencyVertex : DependencyVertex
    {
        public SystemDependencyVertex(string caption)
        {
            this.Shape = VertexShape.Ellipse;
            this.Text = caption;
        }

        public SystemDependencyVertex(Version version)
        {
            this.Shape = VertexShape.Ellipse;
            this.Text = "Windows " + version;
        }
    }
}