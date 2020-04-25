namespace otor.msixhero.lib.Domain.SystemState.ThirdParty
{
    public interface IThirdPartyDetectedApp
    {
        string AppId { get; }
        string Name { get; }
        string Publisher { get; }
        string Version { get; }
        string Website { get; }
    }

    public interface IMsixCreator
    {
        string ProjectExtension { get; }

        void CreateProject(string path, bool open = true);
    }

    public class ThirdPartyDetectedApp : IThirdPartyDetectedApp
    {
        public ThirdPartyDetectedApp(string appId, string name, string publisher, string version, string website)
        {
            this.AppId = appId;
            this.Name = name;
            this.Publisher = publisher;
            this.Version = version;
            this.Website = website;
        }

        public string AppId { get; }

        public string Name { get; }

        public string Publisher { get; }

        public string Version { get; }

        public string Website { get; }
    }
}
