namespace Otor.Msix.Dependencies.Domain
{
    public class Relation
    {
        public Relation(GraphElement left, string relationDescription, GraphElement right)
        {
            this.Left = left;
            this.RelationDescription = relationDescription;
            this.Right = right;
        }

        public GraphElement Left { get; }

        public string RelationDescription { get; }

        public GraphElement Right { get; }

        public override string ToString()
        {
            return this.Left + " " + this.RelationDescription + " " + this.Right;
        }
    }
}