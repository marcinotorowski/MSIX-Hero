using System;

namespace Otor.MsixHero.Appx.Reader.Psf.Entities.Descriptor;

[Serializable]
public class MsixHelperApplicationProxy : BaseApplicationProxy
{
    public override ApplicationProxyType Type => ApplicationProxyType.MsixHelper;
}