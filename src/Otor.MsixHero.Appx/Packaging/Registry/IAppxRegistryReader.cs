using System.Collections.Generic;
using System.Threading;
using ABI.System;

namespace Otor.MsixHero.Appx.Packaging.Registry
{
    public interface IAppxRegistryReader : IDisposable
    {
        /// <summary>
        /// Enumerates sub-keys belonging to a registry with a given path.
        /// </summary>
        /// <param name="registryKeyPath">The parent registry path.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <remarks>The parent registry path must be in the following format: Root\Registry\[Machine|User]\[...]</remarks>
        /// <returns>Asynchronous enumeration of values belonging to the given registry key.</returns>
        IAsyncEnumerable<AppxRegistryKey> EnumerateKeys(string registryKeyPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Enumerates the values belonging to a registry with a given path.
        /// </summary>
        /// <param name="registryKeyPath">The parent registry path.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <remarks>The parent registry path must be in the following format: Root\Registry\[Machine|User]\[...]</remarks>
        /// <returns>Asynchronous enumeration of values belonging to the given registry key.</returns>
        IAsyncEnumerable<AppxRegistryValue> EnumerateValues(string registryKeyPath, CancellationToken cancellationToken = default);
    }
}