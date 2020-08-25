using System;

namespace Otor.MsixHero.Lib.Domain.Commands.Volumes
{
    [Serializable]
    public class SetVolumeFilter : ProxyObject
    {
        public SetVolumeFilter()
        {
        }

        public SetVolumeFilter(string searchKey)
        {
            this.SearchKey = searchKey;
        }

        /// <summary>
        /// The search key, or <c>null</c> if the search key is not to be changed by this action.
        /// </summary>
        public string SearchKey { get; set; }

        public static SetVolumeFilter CreateFrom(string searchKey)
        {
            return new SetVolumeFilter(searchKey);
        }
    }
}
