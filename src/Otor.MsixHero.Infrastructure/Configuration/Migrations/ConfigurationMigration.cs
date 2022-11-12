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

namespace Otor.MsixHero.Infrastructure.Configuration.Migrations;

internal abstract class ConfigurationMigration
{
    protected readonly Configuration Configuration;

    protected ConfigurationMigration(Configuration config)
    {
        this.Configuration = config;
    }

    public abstract int? TargetRevision { get; }

    public bool Migrate()
    {
        if (!this.TargetRevision.HasValue || this.TargetRevision.Value <= 0)
        {
            this.DoMigrate();
            return true;
        }

        if (this.Configuration.Revision < this.TargetRevision.Value)
        {
            this.DoMigrate();
            this.Configuration.Revision = this.TargetRevision.Value;
            return true;
        }

        return false;
    }

    protected abstract void DoMigrate();
}