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

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Otor.MsixHero.Infrastructure.Configuration.ResolvableFolder;

namespace Otor.MsixHero.Infrastructure.Configuration.Migrations;

internal class SigningMigration : ConfigurationMigration
{
    public SigningMigration(Configuration config) : base(config)
    {
    }

    public override int? TargetRevision { get; } = 1;

    protected override void DoMigrate()
    {
        var serializeProfile = JsonConvert.SerializeObject(this.Configuration.Signing);
        var deserializedProfile = JsonConvert.DeserializeObject<SigningProfile>(serializeProfile, new ResolvablePathConverter());
        if (deserializedProfile != null && deserializedProfile?.Source != CertificateSource.Unknown)
        {
            this.Configuration.Signing.Profiles.Add(
                new()
                {
                    Name = "default",
                    Source = deserializedProfile.Source,
                    DeviceGuard = deserializedProfile.DeviceGuard,
                    EncodedPassword = deserializedProfile.EncodedPassword,
                    PfxPath = deserializedProfile.PfxPath,
                    Thumbprint = deserializedProfile.Thumbprint,
                    TimeStampServer = deserializedProfile.TimeStampServer,
                    IsDefault = true
                });
        };
    }
}