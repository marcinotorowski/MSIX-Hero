using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using otor.msixhero.lib.BusinessLayer.Appx.AppAttach;
using otor.msixhero.lib.BusinessLayer.Appx.Packer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Wrappers;

namespace otor.msixhero.lib.tests
{
    [TestFixture]
    [Ignore("Integration")]
    public class AppAttachTests
    {
        [Test]
        public void Setup()
        {
            BundleHelper.SdkPath = @"E:\Visual\Playground\MSIX-Hero\artifacts\redistr\sdk";
            BundleHelper.MsixMgrPath = @"E:\Visual\Playground\MSIX-Hero\artifacts\redistr\msixmgr";
            BundleHelper.TemplatesPath = @"E:\Visual\Playground\MSIX-Hero\artifacts\templates";

            var appAttach = new AppAttach();
            appAttach.CreateVolume(@"E:\temp\MSIX.Commander_1.0.6.0-x64.msix", @"C:\temp\my-disk.vhd", 100, true).GetAwaiter().GetResult();
        }
    }
}
