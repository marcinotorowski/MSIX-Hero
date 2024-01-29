using System;
using System.Linq;
using NUnit.Framework;
using Otor.MsixHero.Appx.Updates;

namespace Otor.MsixHero.Tests.UpdateImpact
{
    [TestFixture]
    public class UpdateImpactTests
    {   
        [Test]
        public void TestUpgradeImpact()
        {
            var names = typeof(UpdateImpactTests).Assembly.GetManifestResourceNames();
            var previous = names.First(a => a.EndsWith(".previous.xml"));
            var next = names.First(a => a.EndsWith(".next.xml"));

            var comparer = new AppxBlockMapUpdateImpactAnalyzer();
            using var stream1 = typeof(UpdateImpactTests).Assembly.GetManifestResourceStream(previous);
            using var stream2 = typeof(UpdateImpactTests).Assembly.GetManifestResourceStream(next);

            if (stream2 == null || stream1 == null)
            {
                throw new InvalidOperationException();
            }
            
            var compare = comparer.Analyze(stream1, stream2).Result;
            
            Assert.That(compare.UpdateImpact, Is.EqualTo(1406036));
            Assert.That(compare.SizeDifference, Is.EqualTo(403522));
            Assert.That(compare.AddedFiles.FileSize, Is.EqualTo(294784));
            Assert.That(compare.AddedFiles.BlockSize, Is.EqualTo(1406036)); 
            Assert.That(compare.AddedFiles.UpdateImpact, Is.EqualTo(108523));
            Assert.That(compare.AddedFiles.FileCount, Is.EqualTo(1));
            Assert.That(compare.AddedFiles.SizeDifference, Is.EqualTo(294784));
            Assert.That(compare.AddedFiles.BlockCount, Is.EqualTo(72));

            Assert.That(compare.DeletedFiles.FileSize, Is.EqualTo(66144));
            Assert.That(compare.DeletedFiles.BlockSize, Is.EqualTo(1270988));
            Assert.That(compare.DeletedFiles.FileCount, Is.EqualTo(2));
            Assert.That(compare.DeletedFiles.BlockCount, Is.EqualTo(67));
            Assert.That(compare.DeletedFiles.UpdateImpact, Is.EqualTo(0));
            Assert.That(compare.DeletedFiles.SizeDifference, Is.EqualTo(-66144));

            Assert.That(compare.ChangedFiles.FileCount, Is.EqualTo(18));
            Assert.That(compare.ChangedFiles.OldPackageFileSize, Is.EqualTo(4023390));
            Assert.That(compare.ChangedFiles.OldPackageBlockCount, Is.EqualTo(71));
            Assert.That(compare.ChangedFiles.OldPackageBlockSize, Is.EqualTo(1337955));
            Assert.That(compare.ChangedFiles.NewPackageFileSize, Is.EqualTo(4198272));
            Assert.That(compare.ChangedFiles.NewPackageBlockCount, Is.EqualTo(73));
            Assert.That(compare.ChangedFiles.NewPackageBlockSize, Is.EqualTo(1395168));
            Assert.That(compare.ChangedFiles.UpdateImpact, Is.EqualTo(1297513));
            Assert.That(compare.ChangedFiles.SizeDifference, Is.EqualTo(174882));
            
            Assert.That(compare.UnchangedFiles.FileSize, Is.EqualTo(64379562));
            Assert.That(compare.UnchangedFiles.BlockSize, Is.EqualTo(23685618));
            Assert.That(compare.UnchangedFiles.FileCount, Is.EqualTo(244));
            Assert.That(compare.UnchangedFiles.BlockCount, Is.EqualTo(1116));
            
            Assert.That(compare.OldPackageLayout.BlockSize, Is.EqualTo(24956606));
            Assert.That(compare.OldPackageLayout.BlockCount, Is.EqualTo(1183));
            Assert.That(compare.OldPackageLayout.FileSize, Is.EqualTo(68469096));
            Assert.That(compare.OldPackageLayout.FileCount, Is.EqualTo(264));
            Assert.That(compare.OldPackageLayout.Size, Is.EqualTo(24975386));
            
            Assert.That(compare.NewPackageLayout.BlockSize, Is.EqualTo(25091654));
            Assert.That(compare.NewPackageLayout.BlockCount, Is.EqualTo(1188));
            Assert.That(compare.NewPackageLayout.FileSize, Is.EqualTo(68872618));
            Assert.That(compare.NewPackageLayout.FileCount, Is.EqualTo(263));
            Assert.That(compare.NewPackageLayout.Size, Is.EqualTo(25110368));
            
            Assert.That(compare.OldPackageDuplication.PossibleSizeReduction, Is.EqualTo(335503));
            Assert.That(compare.OldPackageDuplication.PossibleImpactReduction, Is.EqualTo(103374));
            Assert.That(compare.OldPackageDuplication.FileSize, Is.EqualTo(335503));
            Assert.That(compare.OldPackageDuplication.FileCount, Is.EqualTo(96));
            
            Assert.That(compare.NewPackageDuplication.PossibleSizeReduction, Is.EqualTo(335503));
            Assert.That(compare.NewPackageDuplication.PossibleImpactReduction, Is.EqualTo(103374));
            Assert.That(compare.NewPackageDuplication.FileSize, Is.EqualTo(335503));
            Assert.That(compare.NewPackageDuplication.FileCount, Is.EqualTo(96));
            
        }
    }
}
