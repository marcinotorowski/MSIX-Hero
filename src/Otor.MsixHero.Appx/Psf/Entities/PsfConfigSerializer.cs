using Newtonsoft.Json;

namespace Otor.MsixHero.Appx.Psf.Entities
{
    public class PsfConfigSerializer
    {
        public PsfConfig Deserialize(string jsonString)
        {
            return JsonConvert.DeserializeObject<PsfConfig>(jsonString);
        }
    }
}