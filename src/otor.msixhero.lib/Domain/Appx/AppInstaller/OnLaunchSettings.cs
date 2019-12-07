using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Appx.AppInstaller
{
    public class OnLaunchSettings : IXmlSerializable
    {
        public int HoursBetweenUpdateChecks { get; set; }

        public bool ShowPrompt { get; set; }

        public bool UpdateBlocksActivation { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            int.TryParse(reader.GetAttribute("HoursBetweenUpdateChecks") ?? "0", out var parsedHours);
            bool.TryParse(reader.GetAttribute("ShowPrompt") ?? "False", out var parsedPrompt);
            bool.TryParse(reader.GetAttribute("UpdateBlocksActivation") ?? "False", out var parsedBlock);

            this.ShowPrompt = parsedPrompt;
            this.UpdateBlocksActivation = parsedBlock;
            this.HoursBetweenUpdateChecks = parsedHours;

            reader.ReadStartElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("HoursBetweenUpdateChecks", this.HoursBetweenUpdateChecks.ToString("0"));
            writer.WriteAttributeString("ShowPrompt", this.ShowPrompt.ToString().ToLower());
            writer.WriteAttributeString("UpdateBlocksActivation", this.ShowPrompt.ToString().ToLower());
        }
    }
}