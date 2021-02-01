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

using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Logs;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;

namespace Otor.MsixHero.App.Modules.EventViewer.Commands
{
    public class EventViewerCommandHandler
    {
        private readonly IMsixHeroApplication application;
        private readonly IBusyManager busyManager;
        private readonly IInteractionService interactionService;

        public EventViewerCommandHandler(
            UIElement parent,
            IMsixHeroApplication application,
            IInteractionService interactionService,
            IBusyManager busyManager)
        {
            this.application = application;
            this.interactionService = interactionService;
            this.busyManager = busyManager;

            parent.CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, this.OnRefresh, this.CanRefresh));
            parent.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, this.OnCopy, this.CanCopy));
        }

        private async void OnRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            var executor = this.application.CommandExecutor
                .WithErrorHandling(this.interactionService, true)
                .WithBusyManager(this.busyManager, OperationType.EventsLoading);

            await executor.Invoke(this, new GetLogsCommand(), CancellationToken.None).ConfigureAwait(false);
        }

        private void CanRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OnCopy(object sender, ExecutedRoutedEventArgs e)
        {
            var log = this.application.ApplicationState.EventViewer.SelectedLog;
            if (log != null)
            {
                Clipboard.SetText(GetCopyText(log), TextDataFormat.Text);
            }
        }

        private void CanCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.application.ApplicationState.EventViewer.SelectedLog != null;
        }

        private static string GetCopyText(Log log)
        {
            var copiedText = new StringBuilder();

            if (log.DateTime != default)
            {
                copiedText.AppendFormat("DateTime:\t{0}\r\n", log.DateTime);
            }

            if (!string.IsNullOrEmpty(log.Level))
            {
                copiedText.AppendFormat("Level:\t{0}\r\n", log.Level);
            }

            if (!string.IsNullOrEmpty(log.Source))
            {
                copiedText.AppendFormat("Source:\t{0}\r\n", log.Source);
            }

            if (log.ActivityId != null)
            {
                copiedText.AppendFormat("ActivityId:\t{0}\r\n", log.ActivityId);
            }

            if (!string.IsNullOrEmpty(log.FilePath))
            {
                copiedText.AppendFormat("FilePath:\t{0}\r\n", log.FilePath);
            }

            if (!string.IsNullOrEmpty(log.PackageName))
            {
                copiedText.AppendFormat("PackageName:\t{0}\r\n", log.PackageName);
            }

            if (!string.IsNullOrEmpty(log.ErrorCode))
            {
                copiedText.AppendFormat("ErrorCode:\t{0}\r\n", log.ErrorCode);
            }

            if (!string.IsNullOrEmpty(log.OpcodeDisplayName))
            {
                // ReSharper disable once StringLiteralTypo
                copiedText.AppendFormat("Opcode:\t{0}\r\n", log.OpcodeDisplayName);
            }

            if (log.ThreadId != default)
            {
                copiedText.AppendFormat("ThreadId:\t{0}\r\n", log.ThreadId);
            }

            if (!string.IsNullOrEmpty(log.User))
            {
                copiedText.AppendFormat("User:\t{0}\r\n", log.User);
            }

            if (!string.IsNullOrEmpty(log.Message))
            {
                copiedText.AppendFormat("Message:\t{0}\r\n", log.Message);
            }

            return copiedText.ToString().TrimEnd();
        }
    }
}
