using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ControlzEx;
using ControlzEx.Behaviors;
using ControlzEx.Controls;
using ControlzEx.Native;
using ControlzEx.Standard;
using Microsoft.VisualBasic;
using Interaction = Microsoft.Xaml.Behaviors.Interaction;

namespace Otor.MsixHero.App
{
    internal static class WindowHelper
    {
        public static bool IsWindowHandleValid(IntPtr windowHandle)
        {
            return windowHandle != IntPtr.Zero
#pragma warning disable 618
                   && NativeMethods.IsWindow(windowHandle);
#pragma warning restore 618
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var b = new WindowChromeBehavior();
            b.IgnoreTaskbarOnMaximize = false;
            b.TryToBeFlickerFree = true;
            //b.ResizeBorderThickness = new Thickness(0);
            Interaction.GetBehaviors(this).Add(b);
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                return;
            }
            else
            {
                this.DragMove();
            }
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
        }
    }
}
