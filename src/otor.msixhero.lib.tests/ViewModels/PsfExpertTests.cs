using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel;

namespace otor.msixhero.lib.tests.ViewModels
{
    [TestFixture]
    public class PsfExpertTests
    {
        [Test]
        public void TestBuild()
        {
            var serializer = new PsfConfigSerializer();
            var json = File.ReadAllText(@"E:\temp\msix-psf\fixed-rayeval\config.json");
            var config = serializer.Deserialize(json);

            var builder = new PsfExpertRedirectionViewModelBuilder(config);
            var b = builder.Build();
        }
    }
}
