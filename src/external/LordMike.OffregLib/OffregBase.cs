using System;

namespace OffregLib
{
    public abstract class OffregBase : IDisposable
    {
        /// <summary>
        ///     The internal handle for the unmanaged object.
        /// </summary>
        protected IntPtr _intPtr;

        /// <summary>
        ///     Closes the object and releases the unmanaged ressources.
        /// </summary>
        public abstract void Close();

        /// <summary>
        ///     Disposes the object and releases the unmanaged ressources.
        /// </summary>
        public abstract void Dispose();
    }
}