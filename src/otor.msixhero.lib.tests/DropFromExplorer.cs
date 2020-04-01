using System;
using Moq;
using NUnit.Framework;
using otor.msixhero.ui.Modules;
using otor.msixhero.ui.Modules.Dialogs;
using Prism.Services.Dialogs;

namespace otor.msixhero.lib.tests
{
    [TestFixture]
    public class DropFromExplorer
    {
        [Test]
        public void TestMsixDrop()
        {
            var droppedFiles = new[] {"test1.msix", "test2.msix", "test3.msix"};

            var mock = new Mock<IDialogService>();

            Action<string, IDialogParameters, Action<IDialogResult>> show = (a, b, c) => { };
            Action<string, IDialogParameters, Action<IDialogResult>> showDialog = (a, b, c) => { };

            mock.Setup(d => d.Show(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<Action<IDialogResult>>())).Callback(show);
            mock.Setup(d => d.ShowDialog(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<Action<IDialogResult>>())).Callback(showDialog);

            var handler = new ExplorerHandler(mock.Object);
            handler.Handle(droppedFiles);

            handler = new ExplorerHandler(mock.Object, true);
            handler.Handle(droppedFiles);

            mock.Verify(e => e.Show(Constants.PathPackageExpert, It.IsAny<IDialogParameters>(), It.IsAny<Action<IDialogResult>>()), () => Times.Exactly(3));
            mock.Verify(e => e.ShowDialog(Constants.PathPackageExpert, It.IsAny<IDialogParameters>(), It.IsAny<Action<IDialogResult>>()), () => Times.Exactly(1));
        }
    }
}
