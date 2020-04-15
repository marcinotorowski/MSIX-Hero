namespace otor.msixhero.lib.Domain.Appx.Psf
{
    public class CustomPsfFixupConfig : PsfFixupConfig
    {
        public CustomPsfFixupConfig(string json)
        {
            Json = json;
        }

        public CustomPsfFixupConfig()
        {
        }

        public string Json { get; set; }
    }
}