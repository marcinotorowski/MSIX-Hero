using System;
using System.Collections.Generic;
using System.Text;
using GraphX.Common.Enums;
using GraphX.Common.Models;
using GraphX.Controls;
using GraphX.Logic.Models;
using QuickGraph;

namespace WpfApp1
{
    //Layout visual class
    public class GraphAreaExample : GraphArea<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>
    {
    }

    //Graph data class
    public class GraphExample : BidirectionalGraph<DataVertex, DataEdge> { }

    //Logic core class
    public class GXLogicCoreExample : GXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>> { }


    public class DataVertex : VertexBase
    {
        /// <summary>
        /// Some string property for example purposes
        /// </summary>
        public string Text { get; set; }

        public VertexShape Shape { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class DataEdge : EdgeBase<DataVertex>
    {
        public DataEdge(DataVertex source, DataVertex target, double weight = 1) : base(source, target, weight)
        {
        }

        public DataEdge() : base(null, null, 1)
        {
        }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
