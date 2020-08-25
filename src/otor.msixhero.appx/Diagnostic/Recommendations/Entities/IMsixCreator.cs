namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities
{
    public interface IMsixCreator
    {
        string ProjectExtension { get; }

        void CreateProject(string path, bool open = true);
    }
}