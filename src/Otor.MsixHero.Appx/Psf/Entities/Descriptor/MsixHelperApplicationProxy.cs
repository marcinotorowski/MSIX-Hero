using System;

namespace Otor.MsixHero.Appx.Psf.Entities.Descriptor;

[Serializable]
public class MsixHelperApplicationProxy : BaseApplicationProxy
{
    public override ApplicationProxyType Type => ApplicationProxyType.MsixHelper;
}