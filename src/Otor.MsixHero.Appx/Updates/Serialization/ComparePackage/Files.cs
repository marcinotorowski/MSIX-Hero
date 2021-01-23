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

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Serialization.ComparePackage
{
    public class Files : ISize, IUpdateImpact
    {
        [XmlAttribute]
        public long Size { get; set; }

        [XmlAttribute]
        public long UpdateImpact { get; set; }

        [XmlElement(ElementName = "File")]
        public List<File> Items { get; set; }
    }
}