using System.Xml.Serialization;

namespace otor.msixhero.lib.BusinessLayer.Commands
{
    public abstract class SelfElevatedCommand<T> : BaseCommand<T>
    {
        [XmlIgnore]
        public virtual bool RequiresElevation
        {
            get
            {
                return false;
            }
        }
    }
}