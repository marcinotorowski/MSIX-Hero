using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using otor.msixhero.ui.Modules.Dialogs.CertificateExport.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.Help.View
{
    /// <summary>
    /// Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView : UserControl
    {
        public HelpView()
        {
            this.InitializeComponent();
            this.TextBox.Text = @"Copyright (c) 2020 by Marcin Otorowski

All rights reserved.

Redistribution and use in bytecode or binary forms, without modification, 
are permitted for both commercial and non-commercial use, provided that 
the following conditions are met:

* Redistributions in byte-code or binary form must reproduce the above
  copyright notice, this list of conditions and the following disclaimer
  in the documentation and/or other materials provided with the
  distribution.

* Neither the name of the MSIX Hero nor the names of its author may
  be used to endorse or promote products derived from or distributed
  with this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER AND CONTRIBUTORS ""AS IS""
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.

---

MSIX Hero uses the following third-party software and libraries.

* BouncyCastle (by Legion of the Bouncy Castle Inc)
* ControlzEx (by Jan Karger, Bastian Schmidt, James Willock)
* Extended WPF Toolkit for .NET Core 3.0 (by Xceed, ported by Mikołaj M)
* Fluent Ribbon (by Bastian Schmidt, Degtyarev Daniel, Rikker Serg)
* GongSolutions.WPF.DragDrop (by Jan Karger, Steven Kirk)
* MSIX Package Support Framework (by Microsoft and contributors)
* msixmgr (by Microsoft)
* NLog (by Jaroslaw Kowalski, Kim Christensen, Julian Verdurmen)
* Ookii Dialogs WPF (by Sven Groot)
* Prism (by .NET Foundation)
* Registry (by Eric Zimmerman)
* Windows 10 SDK (selected) (by Microsoft)

See also https://msixhero.net/license/.";
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var p = new ProcessStartInfo("https://msixhero.net/get")
            {
                UseShellExecute = true
            };

            Process.Start(p);
        }

        private void Hyperlink2_OnClick(object sender, RoutedEventArgs e)
        {
            var p = new ProcessStartInfo("https://marcinotorowski.com")
            {
                UseShellExecute = true
            };

            Process.Start(p);
        }
    }
}
