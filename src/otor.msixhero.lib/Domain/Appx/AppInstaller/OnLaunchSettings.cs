using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Appx.AppInstaller
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
            writer.WriteAttributeString("UpdateBlocksActivation", this.ShowPrompt.ToString().ToLower());
        }
    }
}