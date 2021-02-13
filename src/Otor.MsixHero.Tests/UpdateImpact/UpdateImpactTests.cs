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
            
            Assert.AreEqual(1406036, compare.UpdateImpact);
            Assert.AreEqual(403522, compare.SizeDifference);
            Assert.AreEqual(294784, compare.AddedFiles.FileSize);
            Assert.AreEqual(1406036, compare.AddedFiles.BlockSize);
            Assert.AreEqual(108523, compare.AddedFiles.UpdateImpact);
            Assert.AreEqual(1, compare.AddedFiles.FileCount);
            Assert.AreEqual(294784, compare.AddedFiles.SizeDifference);
            Assert.AreEqual(72, compare.AddedFiles.BlockCount);

            Assert.AreEqual(66144, compare.DeletedFiles.FileSize);
            Assert.AreEqual(1270988, compare.DeletedFiles.BlockSize);
            Assert.AreEqual(2, compare.DeletedFiles.FileCount);
            Assert.AreEqual(67, compare.DeletedFiles.BlockCount);
            Assert.AreEqual(0, compare.DeletedFiles.UpdateImpact);
            Assert.AreEqual(-66144, compare.DeletedFiles.SizeDifference);

            Assert.AreEqual(18, compare.ChangedFiles.FileCount);
            Assert.AreEqual(4023390, compare.ChangedFiles.OldPackageFileSize);
            Assert.AreEqual(71, compare.ChangedFiles.OldPackageBlockCount);
            Assert.AreEqual(1337955, compare.ChangedFiles.OldPackageBlockSize);
            Assert.AreEqual(4198272, compare.ChangedFiles.NewPackageFileSize);
            Assert.AreEqual(73, compare.ChangedFiles.NewPackageBlockCount);
            Assert.AreEqual(1395168, compare.ChangedFiles.NewPackageBlockSize);
            Assert.AreEqual(1297513, compare.ChangedFiles.UpdateImpact);
            Assert.AreEqual(174882, compare.ChangedFiles.SizeDifference);
            
            Assert.AreEqual(64379562, compare.UnchangedFiles.FileSize);
            Assert.AreEqual(23685618, compare.UnchangedFiles.BlockSize);
            Assert.AreEqual(244, compare.UnchangedFiles.FileCount);
            Assert.AreEqual(1116, compare.UnchangedFiles.BlockCount);
            
            Assert.AreEqual(24956606, compare.OldPackageLayout.BlockSize);
            Assert.AreEqual(1183, compare.OldPackageLayout.BlockCount);
            Assert.AreEqual(68469096, compare.OldPackageLayout.FileSize);
            Assert.AreEqual(264, compare.OldPackageLayout.FileCount);
            Assert.AreEqual(24975386, compare.OldPackageLayout.Size);
            
            Assert.AreEqual(25091654, compare.NewPackageLayout.BlockSize);
            Assert.AreEqual(1188, compare.NewPackageLayout.BlockCount);
            Assert.AreEqual(68872618, compare.NewPackageLayout.FileSize);
            Assert.AreEqual(263, compare.NewPackageLayout.FileCount);
            Assert.AreEqual(25110368, compare.NewPackageLayout.Size);
            
            Assert.AreEqual(335503, compare.OldPackageDuplication.PossibleSizeReduction);
            Assert.AreEqual(103374, compare.OldPackageDuplication.PossibleImpactReduction);
            Assert.AreEqual(335503, compare.OldPackageDuplication.FileSize);
            Assert.AreEqual(96, compare.OldPackageDuplication.FileCount);
            
            Assert.AreEqual(335503, compare.NewPackageDuplication.PossibleSizeReduction);
            Assert.AreEqual(103374, compare.NewPackageDuplication.PossibleImpactReduction);
            Assert.AreEqual(335503, compare.NewPackageDuplication.FileSize);
            Assert.AreEqual(96, compare.NewPackageDuplication.FileCount);
            
        }
    }
}
