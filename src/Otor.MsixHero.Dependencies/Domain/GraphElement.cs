namespace Otor.MsixHero.Dependencies.Domain
{
    public abstract class GraphElement
    {
        protected GraphElement(int id)
        {
            this.Id = id;
        }

        public int Id { get; }
    }
}