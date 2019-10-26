using System.Xml.Serialization;

namespace otor.msixhero.lib.BusinessLayer.Actions
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