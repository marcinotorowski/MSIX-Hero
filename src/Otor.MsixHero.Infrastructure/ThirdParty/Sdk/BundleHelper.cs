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
using System.IO;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public static class BundleHelper
    {
        static BundleHelper()
        {
            SdkPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redistr", "sdk");
            MsixMgrPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redistr", "msixmgr");
            PsfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redistr", "psf");
            TemplatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates");
        }

        public static string SdkPath { get; set; }

        public static string MsixMgrPath { get; set; }

        public static string PsfPath { get; set; }

        public static string TemplatesPath { get; set; }
    }
}
