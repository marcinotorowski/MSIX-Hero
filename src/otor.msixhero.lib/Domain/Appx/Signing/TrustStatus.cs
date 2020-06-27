﻿using System;

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

        public string Issuer { get; set; }

        public string Thumbprint { get; set; }

        public DateTime? Expires { get; set; }
    }
}
