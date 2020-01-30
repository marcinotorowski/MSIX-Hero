using Newtonsoft.Json;

namespace otor.msixhero.lib.Domain.Appx.Psf
{
    public class PsfConfigSerializer
    {
        public PsfConfig Deserialize(string jsonString)
        {
            return JsonConvert.DeserializeObject<PsfConfig>(jsonString);
        }
    }
}