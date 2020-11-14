using System;
using System.Windows;
using System.Windows.Input;
using ControlzEx.Behaviors;
using ControlzEx.Standard;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Modularity;
using Prism.Services.Dialogs;
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
        public MainWindow(IInteractionService interactionService, IDialogService dialogService, IModuleManager moduleManager)
        {
            InitializeComponent();

            var b = new WindowChromeBehavior();
            b.IgnoreTaskbarOnMaximize = false;
            b.TryToBeFlickerFree = true;
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
