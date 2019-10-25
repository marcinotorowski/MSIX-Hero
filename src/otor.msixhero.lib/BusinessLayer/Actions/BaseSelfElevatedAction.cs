using System.Xml.Serialization;

namespace otor.msixhero.lib.BusinessLayer.Actions
{
    public abstract class BaseElevatedAction : BaseAction
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