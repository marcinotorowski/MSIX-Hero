using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Otor.MsixHero.App.Helpers.Interop
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public static class WindowsIcons
    {
        private static readonly IDictionary<WindowsIconKey, BitmapSource> icons = new Dictionary<WindowsIconKey,BitmapSource>();
        
        public static BitmapSource FolderLarge => GetWindowsIcon(new WindowsIconKey(SHSIID_FOLDER, SHGSI_LARGEICON));
        
        public static BitmapSource FolderSmall => GetWindowsIcon(new WindowsIconKey(SHSIID_FOLDER, SHGSI_SMALLICON));

        public static BitmapSource DocumentLarge => GetWindowsIcon(new WindowsIconKey(SHSIID_DOCUMENT, SHGSI_LARGEICON));
        
        public static BitmapSource DocumentSmall => GetWindowsIcon(new WindowsIconKey(SHSIID_DOCUMENT, SHGSI_SMALLICON));
        
        public static BitmapSource SettingsLarge => GetWindowsIcon(new WindowsIconKey(SHSIID_SETTINGS, SHGSI_LARGEICON));
        
        public static BitmapSource SettingsSmall => GetWindowsIcon(new WindowsIconKey(SHSIID_SETTINGS, SHGSI_SMALLICON));
        
        private static BitmapSource GetWindowsIcon(WindowsIconKey icon)
        {
            if (icons.TryGetValue(icon, out var source))
            {
                return source;
            }
            
            var info = new ShStockIconInfo();
            info.cbSize = (uint)Marshal.SizeOf(info);
            
            SHGetStockIconInfo(icon.Type, SHGSI_ICON | icon.Size, ref info);
            // var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(info.hIcon, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            using var iconDrawing = Icon.FromHandle(info.hIcon);
            using var bitmap = iconDrawing.ToBitmap();

            var hBitmap = bitmap.GetHbitmap();
            try
            {
                BitmapSizeOptions size;
                switch (icon.Size)
                {
                    case SHGSI_SMALLICON:
                        size = BitmapSizeOptions.FromWidthAndHeight(16, 16);
                        break;
                    default:
                        size = BitmapSizeOptions.FromWidthAndHeight(32, 32);
                        break;
                }

                var wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    size);
                wpfBitmap.Freeze();
                
                icons[icon] = wpfBitmap;
                return wpfBitmap;
            }
            finally
            {
                DeleteObject(hBitmap);
                DestroyIcon(info.hIcon);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ShStockIconInfo
        {
            public uint cbSize;
            public IntPtr hIcon;
            public int iSysIconIndex;
            public int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }

        [DllImport("shell32.dll")]
        [Obfuscation(Exclude = true)]
        private static extern int SHGetStockIconInfo(uint siid, uint uFlags, ref ShStockIconInfo psii);

        [DllImport("user32.dll")]
        [Obfuscation(Exclude = true)]
        private static extern bool DestroyIcon(IntPtr handle);
        
        [DllImport("Shell32.dll")]
        public static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref ShellIcon.SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);
        
        [DllImport("gdi32.dll", SetLastError = true)]
        [Obfuscation(Exclude = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        private const uint SHSIID_DOCUMENT = 1;
        
        private const uint SHSIID_FOLDER = 3;
        
        private const uint SHSIID_SETTINGS = 55;
        
        private const uint SHGSI_ICON = 0x100;
        
        private const uint SHGSI_LARGEICON = 0x0;
        
        private const uint SHGSI_SMALLICON = 0x1;

        private readonly struct WindowsIconKey
        {
            private readonly uint type;

            private readonly uint size;

            public WindowsIconKey(uint type, uint size)
            {
                this.type = type;
                this.size = size;
            }

            public uint Type => this.type;

            public uint Size => this.size;

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(obj, null))
                {
                    return false;
                }

                if (obj is WindowsIconKey wik)
                {
                    return wik.size == this.size && wik.type == this.type;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.type, this.size);
            }
        }
    }
}
