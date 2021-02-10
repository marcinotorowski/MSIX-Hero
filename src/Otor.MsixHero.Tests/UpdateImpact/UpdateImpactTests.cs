using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using Namotion.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Otor.MsixHero.Appx.Updates;

namespace Otor.MsixHero.Tests.UpdateImpact
{
    [TestFixture]
    public class UpdateImpactTests
    {   
        [Test]
        public void Test()
        {
            var pkg1 = @"C:\Users\marci\Documents\DEV\MSIX-Hero\Archive\msix-hero-0.6.0.0\AppxBlockMap.xml";
            var pkg2 = @"C:\Users\marci\Documents\DEV\MSIX-Hero\Archive\msix-hero-0.7.0.0\AppxBlockMap.xml";
            
            using var stream1 = File.OpenRead(pkg1);
            using var stream2 = File.OpenRead(pkg2);
            var files1 = new List<AppxFile>(this.ReadFiles(stream1));
            var files2 = new List<AppxFile>(this.ReadFiles(stream2));

            var presentInBoth = new HashSet<string>(files1.Select(f => f.Name).Union(files2.Select(f => f.Name)));
            
            var newFiles = files2.Except(files1).Where(f => !presentInBoth.Contains(f.Name)).ToList();
            var oldFiles = files1.Except(files2).Where(f => !presentInBoth.Contains(f.Name)).ToList();
            var unchangedFiles = files1.Intersect(files2).ToList();
            var changedFiles1 = files1.Except(oldFiles).Except(unchangedFiles).ToList();
            var changedFiles2 = files2.Except(newFiles).Except(unchangedFiles).ToList();
            
            var sumNewFiles = newFiles.Sum(s => s.Size);
            var sumOldFiles = oldFiles.Sum(s => s.Size);
            var sumUnchangedFiles = unchangedFiles.Sum(s => s.Size);
            var sumChangedFiles1 = changedFiles1.Sum(s => s.Size);
            var sumChangedFiles2 = changedFiles2.Sum(s => s.Size);
        }
        
        private IEnumerable<AppxFile> ReadFiles(Stream stream)
        {
            IList<AppxFile> list = new List<AppxFile>();
            var document = XDocument.Load(stream, LoadOptions.None);
            var blockMap = document.Root;
            if (blockMap == null || blockMap.Name.LocalName != "BlockMap")
            {
                return list;
            }
            
            var ns = XNamespace.Get("http://schemas.microsoft.com/appx/2010/blockmap");

            foreach (var item in blockMap.Elements(ns + "File"))
            {
                long.TryParse(item.Attribute("Size")?.Value ?? "0", out var fileSize);

                var fileName = item.Attribute("Name")?.Value;
                var file = new AppxFile(fileName, fileSize);
                
                foreach (var fileBlock in item.Elements(ns + "Block"))
                {
                    long blockLength;
                    var blockSize = fileBlock.Attribute("Size");
                    if (blockSize == null)
                    {
                        blockLength = fileSize;
                    }
                    else
                    {
                        long.TryParse(blockSize.Value, out blockLength);
                    }

                    file.Blocks.Add(new AppxBlock(fileBlock.Attribute("Hash")?.Value, blockLength));
                }

                list.Add(file);
            }

            return list;
        }
    }
}
