using System;
using System.Runtime.InteropServices;
using System.Text;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace OffregLib
{
    public enum RegValueType : uint
    {
        REG_NONE = 0,
        REG_SZ = 1,
        REG_EXPAND_SZ = 2,
        REG_BINARY = 3,
        REG_DWORD = 4,
        REG_DWORD_LITTLE_ENDIAN = 4,
        REG_DWORD_BIG_ENDIAN = 5,
        REG_LINK = 6,
        REG_MULTI_SZ = 7,
        REG_RESOURCE_LIST = 8,
        REG_FULL_RESOURCE_DESCRIPTOR = 9,
        REG_RESOURCE_REQUIREMENTS_LIST = 10,
        REG_QWORD = 11,
        REG_QWORD_LITTLE_ENDIAN = 11
    }

    public enum RegPredefinedKeys
    {
        HKEY_CLASSES_ROOT = unchecked((int)0x80000000),
        HKEY_CURRENT_USER = unchecked((int)0x80000001),
        HKEY_LOCAL_MACHINE = unchecked((int)0x80000002),
        HKEY_USERS = unchecked((int)0x80000003),
        HKEY_PERFORMANCE_DATA = unchecked((int)0x80000004),
        HKEY_CURRENT_CONFIG = unchecked((int)0x80000005),
        HKEY_DYN_DATA = unchecked((int)0x80000006),
        HKEY_CURRENT_USER_LOCAL_SETTINGS = unchecked((int)0x80000007)
    }

    public enum KeyDisposition : long
    {
        REG_CREATED_NEW_KEY = 0x00000001,
        REG_OPENED_EXISTING_KEY = 0x00000002
    }

    public enum KeySecurity
    {
        KEY_QUERY_VALUE = 0x0001,
        KEY_SET_VALUE = 0x0002,
        KEY_ENUMERATE_SUB_KEYS = 0x0008,
        KEY_NOTIFY = 0x0010,
        DELETE = 0x10000,
        STANDARD_RIGHTS_READ = 0x20000,
        KEY_READ = 0x20019,
        KEY_WRITE = 0x20006,
        KEY_ALL_ACCESS = 0xF003F,
        MAXIMUM_ALLOWED = 0x2000000
    }

    [Flags]
    public enum RegOption : uint
    {
        REG_OPTION_RESERVED = 0x00000000,
        REG_OPTION_NON_VOLATILE = 0x00000000,
        REG_OPTION_VOLATILE = 0x00000001,
        REG_OPTION_CREATE_LINK = 0x00000002,
        REG_OPTION_BACKUP_RESTORE = 0x00000004,
        REG_OPTION_OPEN_LINK = 0x00000008
    }

    public enum SECURITY_INFORMATION : uint
    {
        OWNER_SECURITY_INFORMATION = 0x00000001,
        GROUP_SECURITY_INFORMATION = 0x00000002,
        DACL_SECURITY_INFORMATION = 0x00000004,
        SACL_SECURITY_INFORMATION = 0x00000008,
        LABEL_SECURITY_INFORMATION = 0x00000010,
        PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000,
        PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
        UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
        UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000,
    }

    /// <summary>
    ///     All the functions can be read about here:
    ///     http://msdn.microsoft.com/en-us/library/ee210756(v=vs.85).aspx
    /// </summary>
    public static class OffregNative
    {
        private static bool Is64BitProcess { get { return IntPtr.Size == 8; } }

        private const string OffRegDllName32 = "offreg.x86.dll";
        private const string OffRegDllName64 = "offreg.x64.dll";

        [DllImport(OffRegDllName32, EntryPoint = "ORCreateHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result CreateHive32(out IntPtr rootKeyHandle);

        [DllImport(OffRegDllName64, EntryPoint = "ORCreateHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result CreateHive64(out IntPtr rootKeyHandle);

        /// <summary>
        ///     Create a new Registry Hive
        ///     See http://msdn.microsoft.com/en-us/library/2d6dt3kf.aspx
        /// </summary>
        /// <param name="rootKeyHandle">The handle to the new hive.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result CreateHive(out IntPtr rootKeyHandle)
        {
            return Is64BitProcess ? CreateHive64(out rootKeyHandle) : CreateHive32(out rootKeyHandle);
        }

        [DllImport(OffRegDllName32, EntryPoint = "OROpenHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result OpenHive32(string path, out IntPtr rootKeyHandle);

        [DllImport(OffRegDllName64, EntryPoint = "OROpenHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result OpenHive64(string path, out IntPtr rootKeyHandle);

        /// <summary>
        ///     Open an existing Registry Hive
        ///     See http://msdn.microsoft.com/en-us/library/ee210770(v=vs.85).aspx
        /// </summary>
        /// <param name="path">The path to the hive file.</param>
        /// <param name="rootKeyHandle">The handle to an open hive.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result OpenHive(string path, out IntPtr rootKeyHandle)
        {
            return Is64BitProcess
                       ? OpenHive64(path, out rootKeyHandle)
                       : OpenHive32(path, out rootKeyHandle);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORCloseHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result CloseHive32(IntPtr rootKeyHandle);

        [DllImport(OffRegDllName64, EntryPoint = "ORCloseHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result CloseHive64(IntPtr rootKeyHandle);

        /// <summary>
        ///     Close an open hive, freeing ressources used by it.
        ///     See http://msdn.microsoft.com/en-us/library/ee210758(v=vs.85).aspx
        /// </summary>
        /// <seealso cref="SaveHive" />
        /// <remarks>
        ///     This does not save a hive to disk, to preserve changes, see <see cref="SaveHive" />.
        /// </remarks>
        /// <param name="rootKeyHandle">The handle to an open hive.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result CloseHive(IntPtr rootKeyHandle)
        {
            return Is64BitProcess ? CloseHive64(rootKeyHandle) : CloseHive32(rootKeyHandle);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORSaveHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result SaveHive32(
            IntPtr rootKeyHandle,
            string path,
            uint dwOsMajorVersion,
            uint dwOsMinorVersion);

        [DllImport(OffRegDllName64, EntryPoint = "ORSaveHive", CharSet = CharSet.Unicode)]
        private static extern Win32Result SaveHive64(
            IntPtr rootKeyHandle,
            string path,
            uint dwOsMajorVersion,
            uint dwOsMinorVersion);

        /// <summary>
        ///     Save an open hive to disk.
        ///     This saves the hive with a specific compatibility option. See the link below for more details.
        ///     See http://msdn.microsoft.com/en-us/library/ee210773(v=vs.85).aspx
        /// </summary>
        /// <param name="rootKeyHandle">The handle to the open hive</param>
        /// <param name="path">The path to a non-existent file in which to save the hive</param>
        /// <param name="dwOsMajorVersion">The major os version to save the hive for. See summary.</param>
        /// <param name="dwOsMinorVersion">The minor os version to save the hive for. See summary.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result SaveHive(IntPtr rootKeyHandle,
                                           string path,
                                           uint dwOsMajorVersion,
                                           uint dwOsMinorVersion)
        {
            return Is64BitProcess
                       ? SaveHive64(rootKeyHandle, path, dwOsMajorVersion, dwOsMinorVersion)
                       : SaveHive32(rootKeyHandle, path, dwOsMajorVersion, dwOsMinorVersion);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORCloseKey")]
        private static extern Win32Result CloseKey32(IntPtr hKey);

        [DllImport(OffRegDllName64, EntryPoint = "ORCloseKey")]
        private static extern Win32Result CloseKey64(IntPtr hKey);

        /// <summary>
        ///     Close an open key.
        ///     See http://msdn.microsoft.com/en-us/library/ee210759(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">The handle to an open key.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result CloseKey(IntPtr hKey)
        {
            return Is64BitProcess ? CloseKey64(hKey) : CloseKey32(hKey);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORCreateKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result CreateKey32(
            IntPtr hKey,
            string lpSubKey,
            string lpClass,
            RegOption dwOptions,
            /*ref SECURITY_DESCRIPTOR*/ IntPtr lpSecurityDescriptor,
            /*ref IntPtr*/ out IntPtr phkResult,
            out KeyDisposition lpdwDisposition);

        [DllImport(OffRegDllName64, EntryPoint = "ORCreateKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result CreateKey64(
            IntPtr hKey,
            string lpSubKey,
            string lpClass,
            RegOption dwOptions,
            /*ref SECURITY_DESCRIPTOR*/ IntPtr lpSecurityDescriptor,
            /*ref IntPtr*/ out IntPtr phkResult,
            out KeyDisposition lpdwDisposition);

        /// <summary>
        ///     Create a new subkey (or open an existing one) under another key.
        ///     See http://msdn.microsoft.com/en-us/library/ee210761(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open key.</param>
        /// <param name="lpSubKey">Name of the new subkey.</param>
        /// <param name="lpClass">Name of the type of the new subkey.</param>
        /// <param name="dwOptions">Options for the creation.</param>
        /// <param name="lpSecurityDescriptor">Security descripter, may be NULL.</param>
        /// <param name="phkResult">The handle to the newly created key.</param>
        /// <param name="lpdwDisposition">The reuslting disposition.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result CreateKey(IntPtr hKey,
                                            string lpSubKey,
                                            string lpClass,
                                            RegOption dwOptions,
            /*ref SECURITY_DESCRIPTOR*/ IntPtr lpSecurityDescriptor,
            /*ref IntPtr*/ out IntPtr phkResult,
                                            out KeyDisposition lpdwDisposition)
        {
            return Is64BitProcess
                       ? CreateKey64(hKey, lpSubKey, lpClass, dwOptions, lpSecurityDescriptor, out phkResult,
                                     out lpdwDisposition)
                       : CreateKey32(hKey, lpSubKey, lpClass, dwOptions, lpSecurityDescriptor, out phkResult,
                                     out lpdwDisposition);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORDeleteKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result DeleteKey32(
            IntPtr hKey,
            string lpSubKey);

        [DllImport(OffRegDllName64, EntryPoint = "ORDeleteKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result DeleteKey64(
            IntPtr hKey,
            string lpSubKey);

        /// <summary>
        ///     Delete a subkey.
        ///     See http://msdn.microsoft.com/en-us/library/ee210762(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open parent key.</param>
        /// <param name="lpSubKey">Name of the subkey, in the parent, to delete. Null indicates that the parent should be deleted.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result DeleteKey(IntPtr hKey,
                                            string lpSubKey)
        {
            return Is64BitProcess ? DeleteKey64(hKey, lpSubKey) : DeleteKey32(hKey, lpSubKey);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORDeleteValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result DeleteValue32(
            IntPtr hKey,
            string lpValueName);

        [DllImport(OffRegDllName64, EntryPoint = "ORDeleteValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result DeleteValue64(
            IntPtr hKey,
            string lpValueName);

        /// <summary>
        ///     Delete a value under a key.
        ///     See http://msdn.microsoft.com/en-us/library/ee210763(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open parent key.</param>
        /// <param name="lpValueName">Name of the value to delete.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result DeleteValue(IntPtr hKey,
                                              string lpValueName)
        {
            return Is64BitProcess ? DeleteValue64(hKey, lpValueName) : DeleteValue32(hKey, lpValueName);
        }

        [DllImport(OffRegDllName32, EntryPoint = "OREnumKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumKey32(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpName,
            ref uint lpcchName,
            StringBuilder lpClass,
            ref uint lpcchClass,
            ref FILETIME lpftLastWriteTime);

        [DllImport(OffRegDllName64, EntryPoint = "OREnumKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumKey64(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpName,
            ref uint lpcchName,
            StringBuilder lpClass,
            ref uint lpcchClass,
            ref FILETIME lpftLastWriteTime);

        /// <summary>
        ///     Enumerate keys under a parent.
        ///     See http://msdn.microsoft.com/en-us/library/ee210764(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open parent key.</param>
        /// <param name="dwIndex">Index of the child to retrieve.</param>
        /// <param name="lpName">Buffer for the childs name.</param>
        /// <param name="lpcchName">Size of the childs name buffer.</param>
        /// <param name="lpClass">Buffer for the childs class.</param>
        /// <param name="lpcchClass">Size of the childs class buffer.</param>
        /// <param name="lpftLastWriteTime">FileTime structure indicating last write time.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success, Win32Result.ERROR_NO_MORE_ITEMS indicates that no more childs exist.
        /// </returns>
        public static Win32Result EnumKey(IntPtr hKey,
                                          uint dwIndex,
                                          StringBuilder lpName,
                                          ref uint lpcchName,
                                          StringBuilder lpClass,
                                          ref uint lpcchClass,
                                          ref FILETIME lpftLastWriteTime)
        {
            return Is64BitProcess
                       ? EnumKey64(hKey, dwIndex, lpName, ref lpcchName, lpClass, ref lpcchClass, ref lpftLastWriteTime)
                       : EnumKey32(hKey, dwIndex, lpName, ref lpcchName, lpClass, ref lpcchClass, ref lpftLastWriteTime);
        }

        [DllImport(OffRegDllName32, EntryPoint = "OREnumKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumKey32(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpName,
            ref uint lpcchName,
            StringBuilder lpClass,
            IntPtr lpcchClass,
            IntPtr lpftLastWriteTime);

        [DllImport(OffRegDllName64, EntryPoint = "OREnumKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumKey64(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpName,
            ref uint lpcchName,
            StringBuilder lpClass,
            IntPtr lpcchClass,
            IntPtr lpftLastWriteTime);

        /// <summary>
        ///     Enumerate keys under a parent.
        ///     See http://msdn.microsoft.com/en-us/library/ee210764(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open parent key.</param>
        /// <param name="dwIndex">Index of the child to retrieve.</param>
        /// <param name="lpName">Buffer for the childs name.</param>
        /// <param name="lpcchName">Size of the childs name buffer.</param>
        /// <param name="lpClass">Unused - set to Intpr.Zero.</param>
        /// <param name="lpcchClass">Unused - set to Intpr.Zero.</param>
        /// <param name="lpftLastWriteTime">Unused - set to Intpr.Zero.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success, Win32Result.ERROR_NO_MORE_ITEMS indicates that no more childs exist.
        /// </returns>
        public static Win32Result EnumKey(IntPtr hKey,
                                          uint dwIndex,
                                          StringBuilder lpName,
                                          ref uint lpcchName,
                                          StringBuilder lpClass,
                                          IntPtr lpcchClass,
                                          IntPtr lpftLastWriteTime)
        {
            return Is64BitProcess
                       ? EnumKey64(hKey, dwIndex, lpName, ref lpcchName, lpClass, lpcchClass, lpftLastWriteTime)
                       : EnumKey32(hKey, dwIndex, lpName, ref lpcchName, lpClass, lpcchClass, lpftLastWriteTime);
        }

        [DllImport(OffRegDllName32, EntryPoint = "OREnumValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumValue32(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpValueName,
            ref uint lpcchValueName,
            out RegValueType lpType,
            IntPtr lpData,
            ref uint lpcbData);

        [DllImport(OffRegDllName64, EntryPoint = "OREnumValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumValue64(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpValueName,
            ref uint lpcchValueName,
            out RegValueType lpType,
            IntPtr lpData,
            ref uint lpcbData);

        /// <summary>
        ///     Enumerate a keys values.
        ///     See http://msdn.microsoft.com/en-us/library/ee210765(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open parent key.</param>
        /// <param name="dwIndex">Index of the child to retrieve.</param>
        /// <param name="lpValueName">Buffer for the childs name.</param>
        /// <param name="lpcchValueName">Size of the childs name buffer.</param>
        /// <param name="lpType">Value type.</param>
        /// <param name="lpData">Pointer to data buffer.</param>
        /// <param name="lpcbData">Size of data buffer.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result EnumValue(IntPtr hKey,
                                            uint dwIndex,
                                            StringBuilder lpValueName,
                                            ref uint lpcchValueName,
                                            out RegValueType lpType,
                                            IntPtr lpData,
                                            ref uint lpcbData)
        {
            return Is64BitProcess
                       ? EnumValue64(hKey, dwIndex, lpValueName, ref lpcchValueName, out lpType, lpData, ref lpcbData)
                       : EnumValue32(hKey, dwIndex, lpValueName, ref lpcchValueName, out lpType, lpData, ref lpcbData);
        }

        [DllImport(OffRegDllName32, EntryPoint = "OREnumValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumValue32(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpValueName,
            ref uint lpcchValueName,
            IntPtr lpType,
            IntPtr lpData,
            IntPtr lpcbData);

        [DllImport(OffRegDllName64, EntryPoint = "OREnumValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result EnumValue64(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpValueName,
            ref uint lpcchValueName,
            IntPtr lpType,
            IntPtr lpData,
            IntPtr lpcbData);

        /// <summary>
        ///     Enumerate a keys values.
        ///     See http://msdn.microsoft.com/en-us/library/ee210765(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open parent key.</param>
        /// <param name="dwIndex">Index of the child to retrieve.</param>
        /// <param name="lpValueName">Buffer for the childs name.</param>
        /// <param name="lpcchValueName">Size of the childs name buffer.</param>
        /// <param name="lpType">Unused - set to Intpr.Zero.</param>
        /// <param name="lpData">Unused - set to Intpr.Zero.</param>
        /// <param name="lpcbData">Unused - set to Intpr.Zero.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result EnumValue(IntPtr hKey,
                                            uint dwIndex,
                                            StringBuilder lpValueName,
                                            ref uint lpcchValueName,
                                            IntPtr lpType,
                                            IntPtr lpData,
                                            IntPtr lpcbData)
        {
            return Is64BitProcess
                       ? EnumValue64(hKey, dwIndex, lpValueName, ref lpcchValueName, lpType, lpData, lpcbData)
                       : EnumValue32(hKey, dwIndex, lpValueName, ref lpcchValueName, lpType, lpData, lpcbData);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORGetKeySecurity")]
        private static extern Win32Result GetKeySecurity32(
            IntPtr hKey,
            SECURITY_INFORMATION securityInformation,
            IntPtr pSecurityDescriptor,
            ref uint lpcbSecurityDescriptor);

        [DllImport(OffRegDllName64, EntryPoint = "ORGetKeySecurity")]
        private static extern Win32Result GetKeySecurity64(
            IntPtr hKey,
            SECURITY_INFORMATION securityInformation,
            IntPtr pSecurityDescriptor,
            ref uint lpcbSecurityDescriptor);

        /// <summary>
        ///     Gets a keys security descriptor.
        ///     See http://msdn.microsoft.com/en-us/library/ee210766(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open key.</param>
        /// <param name="securityInformation">The type of security information to request.</param>
        /// <param name="pSecurityDescriptor">Pointer to data buffer.</param>
        /// <param name="lpcbSecurityDescriptor">Size of the data buffer.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result GetKeySecurity(IntPtr hKey,
                                                 SECURITY_INFORMATION securityInformation,
                                                 IntPtr pSecurityDescriptor,
                                                 ref uint lpcbSecurityDescriptor)
        {
            return Is64BitProcess
                       ? GetKeySecurity64(hKey, securityInformation, pSecurityDescriptor, ref lpcbSecurityDescriptor)
                       : GetKeySecurity32(hKey, securityInformation, pSecurityDescriptor, ref lpcbSecurityDescriptor);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORGetValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result GetValue32(
            IntPtr hKey,
            string lpSubKey,
            string lpValue,
            out RegValueType pdwType,
            IntPtr pvData,
            ref uint pcbData);

        [DllImport(OffRegDllName64, EntryPoint = "ORGetValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result GetValue64(
            IntPtr hKey,
            string lpSubKey,
            string lpValue,
            out RegValueType pdwType,
            IntPtr pvData,
            ref uint pcbData);

        /// <summary>
        ///     Gets a value under a key.
        ///     See http://msdn.microsoft.com/en-us/library/ee210767(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open key.</param>
        /// <param name="lpSubKey">The name of the subkey under the parent, from which to retrieve the value. May be null.</param>
        /// <param name="lpValue">Name of the value to retrieve.</param>
        /// <param name="pdwType">The type of the value.</param>
        /// <param name="pvData">Pointer to a data buffer.</param>
        /// <param name="pcbData">Size of the data buffer.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result GetValue(IntPtr hKey,
                                           string lpSubKey,
                                           string lpValue,
                                           out RegValueType pdwType,
                                           IntPtr pvData,
                                           ref uint pcbData)
        {
            return Is64BitProcess
                       ? GetValue64(hKey, lpSubKey, lpValue, out pdwType, pvData, ref pcbData)
                       : GetValue32(hKey, lpSubKey, lpValue, out pdwType, pvData, ref pcbData);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORGetValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result GetValue32(
            IntPtr hKey,
            string lpSubKey,
            string lpValue,
            out RegValueType pdwType,
            IntPtr pvData,
            IntPtr pcbData);

        [DllImport(OffRegDllName64, EntryPoint = "ORGetValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result GetValue64(
            IntPtr hKey,
            string lpSubKey,
            string lpValue,
            out RegValueType pdwType,
            IntPtr pvData,
            IntPtr pcbData);

        /// <summary>
        ///     Gets a value under a key.
        ///     See http://msdn.microsoft.com/en-us/library/ee210767(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open key.</param>
        /// <param name="lpSubKey">The name of the subkey under the parent, from which to retrieve the value. May be null.</param>
        /// <param name="lpValue">Name of the value to retrieve.</param>
        /// <param name="pdwType">The type of the value.</param>
        /// <param name="pvData">Unused - set to Intpr.Zero.</param>
        /// <param name="pcbData">Unused - set to Intpr.Zero.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result GetValue(IntPtr hKey,
                                           string lpSubKey,
                                           string lpValue,
                                           out RegValueType pdwType,
                                           IntPtr pvData,
                                           IntPtr pcbData)
        {
            return Is64BitProcess
                       ? GetValue64(hKey, lpSubKey, lpValue, out pdwType, pvData, pcbData)
                       : GetValue32(hKey, lpSubKey, lpValue, out pdwType, pvData, pcbData);
        }

        [DllImport(OffRegDllName32, EntryPoint = "OROpenKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result OpenKey32(
            IntPtr hKey,
            string lpSubKey,
            out IntPtr phkResult);

        [DllImport(OffRegDllName64, EntryPoint = "OROpenKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result OpenKey64(
            IntPtr hKey,
            string lpSubKey,
            out IntPtr phkResult);

        /// <summary>
        ///     Open a subkey.
        ///     See http://msdn.microsoft.com/en-us/library/ee210771(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open parent key.</param>
        /// <param name="lpSubKey">Name of the subkey to open.</param>
        /// <param name="phkResult">Handle to the opened subkey.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result OpenKey(IntPtr hKey,
                                          string lpSubKey,
                                          out IntPtr phkResult)
        {
            return Is64BitProcess
                       ? OpenKey64(hKey, lpSubKey, out phkResult)
                       : OpenKey32(hKey, lpSubKey, out phkResult);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORQueryInfoKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result QueryInfoKey32(
            IntPtr hKey,
            StringBuilder lpClass,
            ref uint lpcchClass,
            ref uint lpcSubKeys,
            ref uint lpcbMaxSubKeyLen,
            ref uint lpcbMaxClassLen,
            ref uint lpcValues,
            ref uint lpcbMaxValueNameLen,
            ref uint lpcbMaxValueLen,
            ref uint lpcbSecurityDescriptor,
            ref FILETIME lpftLastWriteTime);

        [DllImport(OffRegDllName64, EntryPoint = "ORQueryInfoKey", CharSet = CharSet.Unicode)]
        private static extern Win32Result QueryInfoKey64(
            IntPtr hKey,
            StringBuilder lpClass,
            ref uint lpcchClass,
            ref uint lpcSubKeys,
            ref uint lpcbMaxSubKeyLen,
            ref uint lpcbMaxClassLen,
            ref uint lpcValues,
            ref uint lpcbMaxValueNameLen,
            ref uint lpcbMaxValueLen,
            ref uint lpcbSecurityDescriptor,
            ref FILETIME lpftLastWriteTime);

        /// <summary>
        ///     Query details about a key.
        ///     See http://msdn.microsoft.com/en-us/library/ee210772(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open key.</param>
        /// <param name="lpClass">The keys class.</param>
        /// <param name="lpcchClass">The size of the class string in chars.</param>
        /// <param name="lpcSubKeys">The number of subkeys.</param>
        /// <param name="lpcbMaxSubKeyLen">The largest name of a subkey.</param>
        /// <param name="lpcbMaxClassLen">The largest subkey class size.</param>
        /// <param name="lpcValues">The number of values.</param>
        /// <param name="lpcbMaxValueNameLen">The largest name of a value.</param>
        /// <param name="lpcbMaxValueLen">The largest values size.</param>
        /// <param name="lpcbSecurityDescriptor">The size of the security descriptor for this key.</param>
        /// <param name="lpftLastWriteTime">The last time the key was written to.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result QueryInfoKey(IntPtr hKey,
                                               StringBuilder lpClass,
                                               ref uint lpcchClass,
                                               ref uint lpcSubKeys,
                                               ref uint lpcbMaxSubKeyLen,
                                               ref uint lpcbMaxClassLen,
                                               ref uint lpcValues,
                                               ref uint lpcbMaxValueNameLen,
                                               ref uint lpcbMaxValueLen,
                                               ref uint lpcbSecurityDescriptor,
                                               ref FILETIME lpftLastWriteTime)
        {
            return Is64BitProcess
                       ? QueryInfoKey64(hKey, lpClass, ref lpcchClass, ref lpcSubKeys, ref lpcbMaxSubKeyLen,
                                        ref lpcbMaxClassLen, ref lpcValues, ref lpcbMaxValueNameLen, ref lpcbMaxValueLen,
                                        ref lpcbSecurityDescriptor, ref lpftLastWriteTime)
                       : QueryInfoKey32(hKey, lpClass, ref lpcchClass, ref lpcSubKeys, ref lpcbMaxSubKeyLen,
                                        ref lpcbMaxClassLen, ref lpcValues, ref lpcbMaxValueNameLen, ref lpcbMaxValueLen,
                                        ref lpcbSecurityDescriptor, ref lpftLastWriteTime);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORSetValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result SetValue32(
            IntPtr hKey,
            string lpValueName,
            RegValueType dwType,
            IntPtr lpData,
            uint cbData);

        [DllImport(OffRegDllName64, EntryPoint = "ORSetValue", CharSet = CharSet.Unicode)]
        private static extern Win32Result SetValue64(
            IntPtr hKey,
            string lpValueName,
            RegValueType dwType,
            IntPtr lpData,
            uint cbData);

        /// <summary>
        ///     Sets a value.
        ///     See http://msdn.microsoft.com/en-us/library/ee210775(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open key.</param>
        /// <param name="lpValueName">The name of the value.</param>
        /// <param name="dwType">The type of the value.</param>
        /// <param name="lpData">The data buffer to save in the value.</param>
        /// <param name="cbData">The size of the data buffer.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result SetValue(IntPtr hKey,
                                           string lpValueName,
                                           RegValueType dwType,
                                           IntPtr lpData,
                                           uint cbData)
        {
            return Is64BitProcess
                       ? SetValue64(hKey, lpValueName, dwType, lpData, cbData)
                       : SetValue32(hKey, lpValueName, dwType, lpData, cbData);
        }

        [DllImport(OffRegDllName32, EntryPoint = "ORSetKeySecurity")]
        private static extern Win32Result SetKeySecurity32(
            IntPtr hKey,
            SECURITY_INFORMATION securityInformation,
            /*ref IntPtr*/ IntPtr pSecurityDescriptor);

        [DllImport(OffRegDllName64, EntryPoint = "ORSetKeySecurity")]
        private static extern Win32Result SetKeySecurity64(
            IntPtr hKey,
            SECURITY_INFORMATION securityInformation,
            /*ref IntPtr*/ IntPtr pSecurityDescriptor);

        /// <summary>
        ///     Sets a keys security descriptor.
        ///     See http://msdn.microsoft.com/en-us/library/ee210774(v=vs.85).aspx
        /// </summary>
        /// <param name="hKey">Handle to an open key.</param>
        /// <param name="securityInformation">The type of security information to set.</param>
        /// <param name="pSecurityDescriptor">Pointer to data buffer.</param>
        /// <returns>
        ///     <see cref="Win32Result" /> of the result. Win32Result.ERROR_SUCCESS indicates success.
        /// </returns>
        public static Win32Result SetKeySecurity(IntPtr hKey,
                                                 SECURITY_INFORMATION securityInformation,
            /*ref IntPtr*/ IntPtr pSecurityDescriptor)
        {
            return Is64BitProcess
                       ? SetKeySecurity64(hKey, securityInformation, pSecurityDescriptor)
                       : SetKeySecurity32(hKey, securityInformation, pSecurityDescriptor);
        }
    }
}