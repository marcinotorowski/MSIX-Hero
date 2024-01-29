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
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Otor.MsixHero.AppInstaller.Entities
{
    [Serializable]
    [XmlRoot("AppInstaller", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
    public class AppInstallerConfig20172 : AppInstallerConfig
    {
        public AppInstallerConfig20172()
        {
        }

        public AppInstallerConfig20172(AppInstallerConfig config) : base(config)
        {
            if (this.UpdateSettings?.OnLaunch != null)
            {
                // Available from 2018/0
                this.UpdateSettings.OnLaunch.UpdateBlocksActivation = false;
                this.UpdateSettings.OnLaunch.ShowPrompt = false;
            }
        }

        public static Task<AppInstallerConfig20172> FromStream(Stream stream)
        {
            return Task.Run(() =>
            {
                var xmlSerializer = new XmlSerializer(typeof(AppInstallerConfig20172));
                var aic = (AppInstallerConfig20172)xmlSerializer.Deserialize(stream);
                return aic;
            });
        }
    }
}