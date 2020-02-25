using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Commands
{
    public abstract class SelfElevatedCommand<T> : BaseCommand<T>, ISelfElevatedCommand
    {
        [XmlIgnore]
        public virtual SelfElevationType RequiresElevation => SelfElevationType.AsInvoker;
    }

    public abstract class SelfElevatedCommand : BaseCommand, ISelfElevatedCommand
    {
        [XmlIgnore]
        public virtual SelfElevationType RequiresElevation => SelfElevationType.AsInvoker;
    }
}