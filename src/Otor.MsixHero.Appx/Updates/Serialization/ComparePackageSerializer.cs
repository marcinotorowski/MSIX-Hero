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

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Updates.Serialization.ComparePackage;
using File = System.IO.File;

namespace Otor.MsixHero.Appx.Updates.Serialization
{
    public class ComparePackageSerializer
    {
        public SdkComparePackage Deserialize([NotNull] Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            
            using (TextReader tr = new StreamReader(stream))
            {
                return this.Deserialize(tr);
            }
        }

        public SdkComparePackage Deserialize([NotNull] string xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            using (TextReader tr = new StringReader(xml))
            {
                return this.Deserialize(tr);
            }
        }

        public SdkComparePackage Deserialize([NotNull] FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            using (var fs = File.OpenRead(file.FullName))
            {
                return this.Deserialize(fs);
            }
        }

        public SdkComparePackage Deserialize([NotNull] TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var serializer = new XmlSerializer(typeof(SdkComparePackage));

            return (SdkComparePackage)serializer.Deserialize(reader);
        }

        public void Serialize([NotNull] SdkComparePackage package, [NotNull] Stream stream)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }
         
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            
            using (TextWriter textWriter = new StreamWriter(stream))
            {
                this.Serialize(package, textWriter);
            }
        }

        public string Serialize([NotNull] SdkComparePackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            var stringBuilder = new StringBuilder();

            using (TextWriter textWriter = new StringWriter(stringBuilder))
            {
                this.Serialize(package, textWriter);
                return stringBuilder.ToString();
            }
        }

        public void Serialize([NotNull] SdkComparePackage package, [NotNull] TextWriter writer)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var serializer = new XmlSerializer(typeof(SdkComparePackage));
            serializer.Serialize(writer, package);
        }

        public void Serialize([NotNull] SdkComparePackage package, [NotNull] FileInfo file)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (file.Directory != null && !file.Directory.Exists)
            {
                file.Directory.Create();
            }

            using (var fs = File.OpenWrite(file.FullName))
            {
                this.Serialize(package, fs);
            }
        }
    }
}
