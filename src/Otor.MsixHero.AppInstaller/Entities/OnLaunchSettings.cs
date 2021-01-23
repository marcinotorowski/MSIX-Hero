// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Otor.MsixHero.AppInstaller.Entities
{
    public class OnLaunchSettings : IXmlSerializable
    {
        public int? HoursBetweenUpdateChecks { get; set; }

        public bool ShowPrompt { get; set; }

        public bool UpdateBlocksActivation { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var hoursAttribute = reader.GetAttribute("HoursBetweenUpdateChecks");
            if (hoursAttribute != null)
            {
                if (int.TryParse(hoursAttribute ?? "24", out var parsedHours) && parsedHours != 24)
                {
                    this.HoursBetweenUpdateChecks = parsedHours;
                }
            }

            bool.TryParse(reader.GetAttribute("ShowPrompt") ?? "False", out var parsedPrompt);
            this.ShowPrompt = parsedPrompt;

            bool.TryParse(reader.GetAttribute("UpdateBlocksActivation") ?? "False", out var parsedBlock);
            this.UpdateBlocksActivation = parsedBlock;
            
            reader.ReadStartElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            if (this.HoursBetweenUpdateChecks.HasValue && this.HoursBetweenUpdateChecks.Value != 24)
            {
                writer.WriteAttributeString("HoursBetweenUpdateChecks", this.HoursBetweenUpdateChecks.Value.ToString("0"));
            }

            writer.WriteAttributeString("ShowPrompt", this.ShowPrompt.ToString().ToLower());
            writer.WriteAttributeString("UpdateBlocksActivation", this.UpdateBlocksActivation.ToString().ToLower());
        }
    }
}