namespace otor.msixhero.lib.Domain.SystemState.ThirdParty
{
    public interface IMsixCreator
    {
        string ProjectExtension { get; }

        void CreateProject(string path, bool open = true);
    }
}