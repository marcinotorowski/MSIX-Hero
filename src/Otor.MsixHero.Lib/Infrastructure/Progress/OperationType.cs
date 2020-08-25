namespace Otor.MsixHero.Lib.Infrastructure.Progress
{
    /// <summary>
    /// The type of the operation.
    /// </summary>
    /// <remarks>Negative values are meant for local scopes.</remarks>
    public enum OperationType
    {
        PackageLoading =-1,
        VolumeLoading = -2,
        Other = 1
    }
}