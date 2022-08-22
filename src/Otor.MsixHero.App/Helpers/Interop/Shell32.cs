// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Otor.MsixHero.App.Helpers.Interop.Structs;
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable ArrangeTypeMemberModifiers

namespace Otor.MsixHero.App.Helpers.Interop;

internal class Shell32
{
    [DllImport("shell32.dll")]
    [Obfuscation(Exclude = true)]
    public static extern int SHGetStockIconInfo(uint siid, uint uFlags, ref ShStockIconInfo psii);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    [Obfuscation(Exclude = true)]
    public static extern int SHGetFileInfo(string pszPath, int dwFileAttributes, out SHFILEINFO psfi, uint cbfileInfo, SHGFI uFlags);

    [DllImport("gdi32.dll", SetLastError = true)]
    [Obfuscation(Exclude = true)]
    public static extern bool DeleteObject(IntPtr hObject);
}