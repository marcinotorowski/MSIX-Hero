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
        private readonly WindowChromeBehavior behavior;

        public MainWindow(IInteractionService interactionService, IDialogService dialogService, IModuleManager moduleManager)
        {
            InitializeComponent();

            this.behavior = new WindowChromeBehavior();
            this.behavior.IgnoreTaskbarOnMaximize = false;
            this.behavior.TryToBeFlickerFree = true;
            this.behavior.ResizeBorderThickness = this.WindowState == WindowState.Maximized ? new Thickness(0) : SystemParameters.WindowResizeBorderThickness;
            Interaction.GetBehaviors(this).Add(this.behavior);

            this.StateChanged += OnStateChanged;
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            this.behavior.ResizeBorderThickness = this.WindowState == WindowState.Maximized ? new Thickness(0) : SystemParameters.WindowResizeBorderThickness;
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
