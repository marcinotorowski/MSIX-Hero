using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Otor.MsixHero.App.Helpers.Interop.Structs;
using Otor.MsixHero.Infrastructure.Extensions;
using static Otor.MsixHero.App.Helpers.Interop.User32;

namespace Otor.MsixHero.App.Helpers.Interop
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public static class WindowsIcons
    {
        private static readonly Lazy<BitmapSource> LazyShieldIcon = new Lazy<BitmapSource>(GetShieldIcon, false);

        private static readonly IDictionary<WindowsIconKey, BitmapSource> icons = new Dictionary<WindowsIconKey,BitmapSource>();

        public static BitmapSource UacShield => LazyShieldIcon.Value;

        private static readonly Dictionary<string, ImageSource> IconSourceCache = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);
        
        private static BitmapSource GetShieldIcon()
        {
            var image = LoadImage(IntPtr.Zero, "#106", 1, (int)SystemParameters.SmallIconWidth, (int)SystemParameters.SmallIconHeight, 0);
            var imageSource = Imaging.CreateBitmapSourceFromHIcon(image, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return imageSource;
        }

        public static ImageSource GetIconFor(string filePath, bool preferCache = true)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            filePath = new FileInfo(filePath).ToFullPath().FullName;

            if (!File.Exists(filePath))
            {
                return null;
            }

            if (preferCache && IconSourceCache.TryGetValue(filePath, out var source))
            {
                return source;
            }

            var info = new SHFILEINFO(true);
            var cbFileInfo = Marshal.SizeOf(info);

            Shell32.SHGetFileInfo(filePath, 1, out info, (uint)cbFileInfo, SHGFI.Icon | SHGFI.LargeIcon | SHGFI.UseFileAttributes);
            ImageSource img = Imaging.CreateBitmapSourceFromHIcon(info.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            User32.DestroyIcon(info.hIcon);
            IconSourceCache[filePath] = img;
            return img;
        }

        public static BitmapSource FolderLarge => GetWindowsIcon(new WindowsIconKey(Flags.SHSIID_FOLDER, Flags.SHGSI_LARGEICON));
        
        public static BitmapSource FolderSmall => GetWindowsIcon(new WindowsIconKey(Flags.SHSIID_FOLDER, Flags.SHGSI_SMALLICON));

        public static BitmapSource DocumentLarge => GetWindowsIcon(new WindowsIconKey(Flags.SHSIID_DOCUMENT, Flags.SHGSI_LARGEICON));
        
        public static BitmapSource DocumentSmall => GetWindowsIcon(new WindowsIconKey(Flags.SHSIID_DOCUMENT, Flags.SHGSI_SMALLICON));
        
        public static BitmapSource SettingsLarge => GetWindowsIcon(new WindowsIconKey(Flags.SHSIID_SETTINGS, Flags.SHGSI_LARGEICON));
        
        public static BitmapSource SettingsSmall => GetWindowsIcon(new WindowsIconKey(Flags.SHSIID_SETTINGS, Flags.SHGSI_SMALLICON));
        
        private static BitmapSource GetWindowsIcon(WindowsIconKey icon)
        {
            if (icons.TryGetValue(icon, out var source))
            {
                return source;
            }
            
            var info = new ShStockIconInfo();
            info.cbSize = (uint)Marshal.SizeOf(info);
            
            Shell32.SHGetStockIconInfo(icon.Type, Flags.SHGSI_ICON | icon.Size, ref info);
            // var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(info.hIcon, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            using var iconDrawing = Icon.FromHandle(info.hIcon);
            using var bitmap = iconDrawing.ToBitmap();

            var hBitmap = bitmap.GetHbitmap();
            try
            {
                BitmapSizeOptions size;
                switch (icon.Size)
                {
                    case Flags.SHGSI_SMALLICON:
                        size = BitmapSizeOptions.FromWidthAndHeight(16, 16);
                        break;
                    default:
                        size = BitmapSizeOptions.FromWidthAndHeight(32, 32);
                        break;
                }

                var wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, size);
                wpfBitmap.Freeze();
                
                icons[icon] = wpfBitmap;
                return wpfBitmap;
            }
            finally
            {
                Shell32.DeleteObject(hBitmap);
                DestroyIcon(info.hIcon);
            }
        }
        
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

    internal class Flags
    {
        public const uint SHSIID_DOCUMENT = 1;
        public const uint SHSIID_FOLDER = 3;
        public const uint SHSIID_SETTINGS = 55;
        public const uint SHGSI_ICON = 0x100;
        public const uint SHGSI_LARGEICON = 0x0;
        public const uint SHGSI_SMALLICON = 0x1;
    }
}
