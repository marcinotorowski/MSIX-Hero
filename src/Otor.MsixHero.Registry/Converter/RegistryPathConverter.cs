// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using Otor.MsixHero.Registry.Parser;

namespace Otor.MsixHero.Registry.Converter
{
    public static class RegistryPathConverter
    {
        public static (RegistryRoot, string) ToCanonicalRegistryPath(string path)
        {
            if (path.StartsWith("REGISTRY\\MACHINE\\", StringComparison.OrdinalIgnoreCase))
            {
                return (RegistryRoot.HKEY_LOCAL_MACHINE, path.Substring("REGISTRY\\MACHINE\\".Length));
            }

            if (path.StartsWith("REGISTRY\\USER\\[{CurrentUserSID}]\\", StringComparison.OrdinalIgnoreCase))
            {
                return (RegistryRoot.HKEY_CURRENT_USER, path.Substring("REGISTRY\\USER\\[{CurrentUserSID}]\\".Length));
            }

            if (path.StartsWith("HKLM", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("MACHINE", StringComparison.OrdinalIgnoreCase))
            {
                return (RegistryRoot.HKEY_LOCAL_MACHINE, path.Substring(path.IndexOf('\\') + 1));
            }

            if (path.StartsWith("HKCU", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("USER", StringComparison.OrdinalIgnoreCase))
            {
                return (RegistryRoot.HKEY_CURRENT_USER, path.Substring(path.IndexOf('\\') + 1));
            }

            if (path.StartsWith("HKCR", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("HKEY_CLASSES_ROOT", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("CLASSES", StringComparison.OrdinalIgnoreCase))
            {
                return (RegistryRoot.HKEY_CLASSES_ROOT, path.Substring(path.IndexOf('\\') + 1));
            }

            throw new ArgumentException("Value is not supported.", nameof(path));
        }

        public static string ToMsixRegistryPath(RegistryRoot root, string path)
        {
            switch (root)
            {
                case RegistryRoot.HKEY_CLASSES_ROOT:
                    return "REGISTRY\\MACHINE\\Software\\Classes\\" + path;
                case RegistryRoot.HKEY_CURRENT_USER:
                    return "REGISTRY\\USER\\[{CurrentUserSID}]\\" + path;
                case RegistryRoot.HKEY_USERS:
                    return "REGISTRY\\USER\\" + path;
                default:
                    return "REGISTRY\\MACHINE\\" + path;
            }
        }
        
        public static string ToMsixRegistryPath(RegistryEntry key)
        {
            return ToMsixRegistryPath(key.Root, key.Key);
        }

        public static string ToMsixRegistryPath(string path)
        {
            if (path.StartsWith("REGISTRY\\", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            if (path.StartsWith("HKLM", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("MACHINE", StringComparison.OrdinalIgnoreCase))
            {
                return ToMsixRegistryPath(RegistryRoot.HKEY_LOCAL_MACHINE, path.Substring(path.IndexOf('\\') + 1));
            }

            if (path.StartsWith("HKCU", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("USER", StringComparison.OrdinalIgnoreCase))
            {
                return ToMsixRegistryPath(RegistryRoot.HKEY_USERS, path.Substring(path.IndexOf('\\') + 1));
            }

            if (path.StartsWith("HKCR", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("HKEY_CLASSES_ROOT", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("CLASSES", StringComparison.OrdinalIgnoreCase))
            {
                return ToMsixRegistryPath(RegistryRoot.HKEY_CLASSES_ROOT, path.Substring(path.IndexOf('\\') + 1));
            }

            return ToMsixRegistryPath(RegistryRoot.HKEY_LOCAL_MACHINE, path);
        }
    }
}