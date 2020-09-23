using System;
using System.ComponentModel;

namespace OffregLib
{
    public class OffregHive : OffregBase
    {
        /// <summary>
        ///     The Root key of this hive.
        /// </summary>
        public OffregKey Root { get; private set; }

        /// <summary>
        ///     Internal constructor to form an Offline Registry Hive from an open handle.
        /// </summary>
        /// <param name="hivePtr"></param>
        internal OffregHive(IntPtr hivePtr)
        {
            _intPtr = hivePtr;

            // Represent this as a key also
            Root = new OffregKey(null, _intPtr, null);
        }

        /// <summary>
        ///     Saves a hive to Disk.
        ///     See http://msdn.microsoft.com/en-us/library/ee210773(v=vs.85).aspx for more details.
        /// </summary>
        /// <remarks>The target file must not exist.</remarks>
        /// <param name="targetFile">The target file to write to.</param>
        /// <param name="majorVersionTarget">The compatibility version to save for, see the link in summary.</param>
        /// <param name="minorVersionTarget">The compatibility version to save for, see the link in summary.</param>
        public void SaveHive(string targetFile, uint majorVersionTarget, uint minorVersionTarget)
        {
            Win32Result res = OffregNative.SaveHive(_intPtr, targetFile, majorVersionTarget, minorVersionTarget);

            if (res != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)res);
        }

        /// <summary>
        ///     Creates a new hive in memory.
        /// </summary>
        /// <returns>The newly created hive.</returns>
        public static OffregHive Create()
        {
            IntPtr newHive;
            Win32Result res = OffregNative.CreateHive(out newHive);

            if (res != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)res);

            return new OffregHive(newHive);
        }

        /// <summary>
        ///     Opens an existing hive from the disk.
        /// </summary>
        /// <param name="hiveFile">The file to open.</param>
        /// <returns>The newly opened hive.</returns>
        public static OffregHive Open(string hiveFile)
        {
            IntPtr existingHive;
            Win32Result res = OffregNative.OpenHive(hiveFile, out existingHive);

            if (res != Win32Result.ERROR_SUCCESS)
                throw new Win32Exception((int)res);

            return new OffregHive(existingHive);
        }

        /// <summary>
        ///     Closes the hive and releases ressources used by it.
        /// </summary>
        public override void Close()
        {
            if (_intPtr != IntPtr.Zero)
            {
                Win32Result res = OffregNative.CloseHive(_intPtr);

                if (res != Win32Result.ERROR_SUCCESS)
                    throw new Win32Exception((int)res);
            }
        }

        /// <summary>
        ///     Disposes the hive object and releases ressources used by it.
        /// </summary>
        public override void Dispose()
        {
            Close();
        }
    }
}