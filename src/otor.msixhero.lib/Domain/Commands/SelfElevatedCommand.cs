using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Commands
{
    public abstract class SelfElevatedCommand<T> : BaseCommand<T>, ISelfElevatedCommand
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

    public abstract class SelfElevatedCommand : BaseCommand, ISelfElevatedCommand
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