// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Diagnostic.Events;
using Otor.MsixHero.Appx.Diagnostic.Events.Entities;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.EventViewer.Commands
{
    public class EventViewerCommandHandler
    {
        private readonly IMsixHeroApplication _application;
        private readonly IBusyManager _busyManager;
        private readonly IInteractionService _interactionService;

        public EventViewerCommandHandler(
            UIElement parent,
            IMsixHeroApplication application,
            IInteractionService interactionService,
            IBusyManager busyManager)
        {
            this._application = application;
            this._interactionService = interactionService;
            this._busyManager = busyManager;

            parent.CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, this.OnRefresh, this.CanRefresh));
            parent.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, this.OnCopy, this.CanCopy));
        }

        private async void OnRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            var executor = this._application.CommandExecutor
                .WithErrorHandling(this._interactionService, true)
                .WithBusyManager(this._busyManager, OperationType.EventsLoading);

            await executor.Invoke<GetEventsCommand, IList<AppxEvent>>(this, new GetEventsCommand(this._application.ApplicationState.EventViewer.Criteria), CancellationToken.None).ConfigureAwait(false);
        }

        private void CanRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OnCopy(object sender, ExecutedRoutedEventArgs e)
        {
            var log = this._application.ApplicationState.EventViewer.SelectedAppxEvent;
            if (log != null)
            {
                Clipboard.SetText(GetCopyText(log), TextDataFormat.Text);
            }
        }

        private void CanCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this._application.ApplicationState.EventViewer.SelectedAppxEvent != null;
        }

        private static string GetCopyText(AppxEvent appxEvent)
        {
            var copiedText = new StringBuilder();

            if (appxEvent.DateTime != default)
            {
                copiedText.AppendFormat("DateTime:\t{0}\r\n", appxEvent.DateTime);
            }

            if (!string.IsNullOrEmpty(appxEvent.Level))
            {
                copiedText.AppendFormat("Level:\t{0}\r\n", appxEvent.Level);
            }

            if (!string.IsNullOrEmpty(appxEvent.Source))
            {
                copiedText.AppendFormat("Source:\t{0}\r\n", appxEvent.Source);
            }

            if (appxEvent.ActivityId != null)
            {
                copiedText.AppendFormat("ActivityId:\t{0}\r\n", appxEvent.ActivityId);
            }

            if (!string.IsNullOrEmpty(appxEvent.FilePath))
            {
                copiedText.AppendFormat("FilePath:\t{0}\r\n", appxEvent.FilePath);
            }

            if (!string.IsNullOrEmpty(appxEvent.PackageName))
            {
                copiedText.AppendFormat("PackageName:\t{0}\r\n", appxEvent.PackageName);
            }

            if (!string.IsNullOrEmpty(appxEvent.ErrorCode))
            {
                copiedText.AppendFormat("ErrorCode:\t{0}\r\n", appxEvent.ErrorCode);
            }

            if (!string.IsNullOrEmpty(appxEvent.OpcodeDisplayName))
            {
                // ReSharper disable once StringLiteralTypo
                copiedText.AppendFormat("Opcode:\t{0}\r\n", appxEvent.OpcodeDisplayName);
            }

            if (appxEvent.ThreadId != default)
            {
                copiedText.AppendFormat("ThreadId:\t{0}\r\n", appxEvent.ThreadId);
            }

            if (!string.IsNullOrEmpty(appxEvent.User))
            {
                copiedText.AppendFormat("User:\t{0}\r\n", appxEvent.User);
            }

            if (!string.IsNullOrEmpty(appxEvent.Message))
            {
                copiedText.AppendFormat("Message:\t{0}\r\n", appxEvent.Message);
            }

            if (appxEvent.ErrorCode != null && appxEvent.ErrorCode.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                var errorCodes = new ErrorCodes();

                string text = null;
                if (ExceptionGuard.Guard(() => errorCodes.TryGetCode(Convert.ToUInt32(appxEvent.ErrorCode, 16), out text)) && !string.IsNullOrEmpty(text))
                {
                    if (errorCodes.TryGetCode(Convert.ToUInt32(appxEvent.ErrorCode, 16), out var message))
                    {
                        text += " | " + message;
                    }

                    copiedText.AppendFormat("Troubleshooting info: {0}", text);
                }
            }
            
            return copiedText.ToString().TrimEnd();
        }
    }
}
