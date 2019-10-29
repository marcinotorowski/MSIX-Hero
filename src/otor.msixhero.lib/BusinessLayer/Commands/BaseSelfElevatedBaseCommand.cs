using System.Xml.Serialization;

namespace otor.msixhero.lib.BusinessLayer.Commands
{
    public abstract class BaseSelfElevatedBaseCommand : BaseCommand
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