using System;
using System.IO;
using System.Threading.Tasks;
using OffregLib;

namespace Otor.MsixHero.Registry.Reader
{
    public class OffregRegistryHive : IRegHive
    {
        private readonly OffregHive hive;

        public OffregRegistryHive(OffregHive hive)
        {
            this.hive = hive;
            this.Root = new OffregRegistryKey(null, hive.Root);
        }

        public IRegKey Root { get; }

        public void Dispose()
        {
            this.hive.Dispose();
        }

        public Task Save(string fileName, Version compatibility = null)
        {
            return Task.Run(() =>
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                if (compatibility == null)
                {
                    this.hive.SaveHive(fileName, 6, 1);
                }
                else
                {
                    this.hive.SaveHive(fileName, (uint)compatibility.Major, (uint)compatibility.Minor);
                }
            });
        }
    }
}