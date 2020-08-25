namespace Otor.MsixHero.Ui.Hero.Events.Base
{
    public abstract class UiPayload
    {
        protected UiPayload(object sender)
        {
            Sender = sender;
        }

        public object Sender { get; }
    }
}