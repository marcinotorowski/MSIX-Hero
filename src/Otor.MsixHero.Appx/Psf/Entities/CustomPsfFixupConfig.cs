namespace Otor.MsixHero.Appx.Psf.Entities
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