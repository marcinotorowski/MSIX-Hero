using System.Runtime.Serialization;

namespace otor.msixhero.lib.Domain.Appx.Psf
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