using System;
using System.Threading.Tasks;

namespace Otor.MsixHero.Registry.Reader
{
    public interface IRegHive : IDisposable
    {
        IRegKey Root { get; }

        Task Save(string fileName, Version compatibility = null);
    }
}