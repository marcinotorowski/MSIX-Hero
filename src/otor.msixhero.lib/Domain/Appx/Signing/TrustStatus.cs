namespace otor.msixhero.lib.Domain.Appx.Signing
{
    public class TrustStatus
    {
        public TrustStatus(bool isTrusted = false, string trustee = null)
        {
            IsTrusted = isTrusted;
            Trustee = trustee;
        }

        public bool IsTrusted { get; private set; }

        public string Trustee { get; private set; }
    }
}
