using System.Runtime.Serialization;

namespace Otor.MsixHero.Appx.Psf.Entities
{
    public enum TraceLevel
    {
        [EnumMember(Value = "unexpectedFailures")]
        UnexpectedFailures,

        [EnumMember(Value = "always")]
        Always,

        [EnumMember(Value = "ignoreSuccess")]
        IgnoreSuccess,

        [EnumMember(Value = "allFailures")]
        AllFailures,

        [EnumMember(Value = "ignore")]
        Ignore
    }
}