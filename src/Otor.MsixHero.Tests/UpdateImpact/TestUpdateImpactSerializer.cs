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

using NUnit.Framework;
using Otor.MsixHero.Appx.Updates.Serialization;

namespace Otor.MsixHero.Tests.UpdateImpact
{
    [TestFixture]
    public class TestUpdateImpactSerializer
    {
        [Test]
        public void TestDeserializing()
        {
            var manifestedResourceName = this.GetType().Namespace + ".update.xml";
            using (var res = typeof(TestUpdateImpactSerializer).Assembly.GetManifestResourceStream(manifestedResourceName))
            {
                var serializer = new ComparePackageSerializer();
                var deserialized = serializer.Deserialize(res);

                Assert.NotNull(deserialized);
                Assert.NotNull(deserialized.Original);
                Assert.NotNull(deserialized.New);
                Assert.NotNull(deserialized.Package);

                Assert.AreEqual("AppxBlockMap.xml", deserialized.Original.Name);
                Assert.AreEqual("0.0.0.0", deserialized.Original.Version);

                Assert.AreEqual("AppxBlockMap.xml", deserialized.New.Name);
                Assert.AreEqual("0.0.0.0", deserialized.New.Version);

                Assert.AreEqual(713881L, deserialized.Package.SizeDifference);
                Assert.AreEqual(499584L, deserialized.Package.AddedSize);
                Assert.AreEqual(66144L, deserialized.Package.DeletedSize);
                Assert.AreEqual(1523045L, deserialized.Package.UpdateImpact);
                Assert.AreEqual(69182977L, deserialized.Package.Size);

                Assert.NotNull(deserialized.Package.ChangedFiles);
                Assert.NotNull(deserialized.Package.DuplicateFiles);
                Assert.NotNull(deserialized.Package.AddedFiles);
                Assert.NotNull(deserialized.Package.DeletedFiles);
                Assert.NotNull(deserialized.Package.UnchangedFiles);

                Assert.AreEqual(280441L, deserialized.Package.ChangedFiles.SizeDifference);
                Assert.AreEqual(1331111L, deserialized.Package.ChangedFiles.UpdateImpact);

                Assert.AreEqual(499584L, deserialized.Package.AddedFiles.Size);
                Assert.AreEqual(191934L, deserialized.Package.AddedFiles.UpdateImpact);

                Assert.AreEqual(66144L, deserialized.Package.DeletedFiles.Size);
                Assert.AreEqual(0L, deserialized.Package.DeletedFiles.UpdateImpact);

                Assert.AreEqual(64379562L, deserialized.Package.UnchangedFiles.Size);
                Assert.AreEqual(0L, deserialized.Package.UnchangedFiles.UpdateImpact);

                Assert.AreEqual(335503L, deserialized.Package.DuplicateFiles.PossibleSizeReduction);
                Assert.AreEqual(103374L, deserialized.Package.DuplicateFiles.PossibleImpactReduction);

                Assert.Greater(deserialized.Package.ChangedFiles.Items.Count, 0);
                var firstChanged = deserialized.Package.ChangedFiles.Items[0];
                Assert.AreEqual("msixhero.dll", firstChanged.Name);
                Assert.AreEqual(115712L, firstChanged.SizeDifference);
                Assert.AreEqual(319595L, firstChanged.UpdateImpact);
                
                Assert.Greater(deserialized.Package.AddedFiles.Items.Count, 0);
                var firstAdded = deserialized.Package.AddedFiles.Items[0];
                Assert.AreEqual("System.Text.Json.dll", firstAdded.Name);
                Assert.AreEqual(294784L, firstAdded.Size);
                Assert.AreEqual(108523L, firstAdded.UpdateImpact);

                Assert.Greater(deserialized.Package.DeletedFiles.Items.Count, 0);
                var firstDeleted = deserialized.Package.DeletedFiles.Items[0];
                Assert.AreEqual("System.Windows.Interactivity.dll", firstDeleted.Name);
                Assert.AreEqual(55904L, firstDeleted.Size);
                Assert.AreEqual(0L, firstDeleted.UpdateImpact);

                Assert.Greater(deserialized.Package.UnchangedFiles.Items.Count, 0);
                var firstUnchanged = deserialized.Package.UnchangedFiles.Items[0];
                Assert.AreEqual(@"runtimes\win\lib\netcoreapp3.1\System.Management.Automation.dll", firstUnchanged.Name);
                Assert.AreEqual(7273344L, firstUnchanged.Size);
                Assert.AreEqual(0L, firstUnchanged.UpdateImpact);
                
                Assert.Greater(deserialized.Package.DuplicateFiles.Items.Count, 0);
                var firstDuplicated = deserialized.Package.DuplicateFiles.Items[0];
                Assert.AreEqual(67584L, firstDuplicated.PossibleSizeReduction);
                Assert.AreEqual(8863L, firstDuplicated.PossibleImpactReduction);
                Assert.Greater(firstDuplicated.Items.Count, 0);
                var firstDuplicatedFile = firstDuplicated.Items[0];
                Assert.AreEqual(@"redistr\sdk\x64\en-US\AppxPackaging.dll.mui", firstDuplicatedFile.Name);
                Assert.AreEqual(67584L, firstDuplicatedFile.Size);
                Assert.AreEqual(8863L, firstDuplicatedFile.DuplicateImpact);
            }
        }
    }
}
