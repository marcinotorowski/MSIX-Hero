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
using Newtonsoft.Json;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Diagnostic.Logging;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels
{
    public class LogViewModel : NotifyPropertyChanged
    {
        private static readonly ErrorCodes ErrorCodes = new();

        public LogViewModel(Log model)
        {
            this.Model = model;
        }

        public Log Model { get; }

        public int ThreadId => this.Model.ThreadId;

        public string User => this.Model.User;

        public Guid? ActivityId => this.Model.ActivityId;

        public string Header
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.ErrorCode))
                {
                    return $"Error {this.ErrorCode}";
                }

                return this.DisplayedLevel;
            }
        }

        public DateTime DateTime => this.Model.DateTime;
        
        public string ErrorCode => this.Model.ErrorCode;

        public string Message => this.Model.Message;

        public string CompactMessage
        {
            get
            {
                string msg;
                if (this.Model.Message.IndexOf(Environment.NewLine, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    msg = this.Model.Message.Remove(this.Model.Message.IndexOf(Environment.NewLine, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    msg = this.Model.Message;
                }

                if (msg.Length > 300)
                {
                    return msg + " [...]";
                }

                return msg;
            }
        }

        public bool HasTranslatedErrorCode
        {
            get
            {
                if (this.Model.ErrorCode == null || !this.Model.ErrorCode.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return ExceptionGuard.Guard(() => ErrorCodes.TryGetCode(Convert.ToUInt32(this.Model.ErrorCode, 16), out _));
            }
        }
        
        public string TroubleshootingTitle
        {
            get
            {
                if (this.Model.ErrorCode == null || !this.Model.ErrorCode.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return ExceptionGuard.Guard(() =>
                {
                    var hasTitle = ErrorCodes.TryGetTitle(Convert.ToUInt32(this.Model.ErrorCode, 16), out var translatedTitle);
                    
                    if (hasTitle)
                    {
                        return translatedTitle;
                    }

                    return null;
                });
            }
        }
        
        public string TroubleshootingDescription
        {
            get
            {
                if (this.Model.ErrorCode == null || !this.Model.ErrorCode.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return ExceptionGuard.Guard(() =>
                {
                    var hasDescription = ErrorCodes.TryGetDescription(Convert.ToUInt32(this.Model.ErrorCode, 16), out var translatedDescription);
                    
                    if (hasDescription)
                    {
                        return translatedDescription;
                    }

                    return null;
                });
            }
        }

        public string TranslatedErrorCode 
        {
            get
            {
                if (this.Model.ErrorCode == null || !this.Model.ErrorCode.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return ExceptionGuard.Guard(() =>
                {
                    if (ErrorCodes.TryGetCode(Convert.ToUInt32(this.Model.ErrorCode, 16), out var translated))
                    {
                        return translated;
                    }

                    return null;
                });
            }
        }

        public string PackageName => this.Model.PackageName;

        public string FilePath => this.Model.FilePath;

        public bool HasUniqueFilePath => !string.IsNullOrWhiteSpace(this.Model.FilePath) && this.Model.FilePath != this.Model.PackageName;

        public string Level => this.Model.Level;

        public string DisplayedLevel
        {
            get
            {
                switch (this.Model.Level.ToLowerInvariant())
                {
                    case "error":
                        return Resources.Localization.EventViewer_Filter_Log_Error;
                    case "warning":
                        return Resources.Localization.EventViewer_Filter_Log_Warn;
                    case "verbose":
                        return Resources.Localization.EventViewer_Filter_Log_Verbose;
                    case "information":
                        return Resources.Localization.EventViewer_Filter_Log_Info;
                    default:
                        return this.Model.Level;
                }
            }
        }

        public string Source => this.Model.Source;

        public string OpcodeDisplayName => this.Model.OpcodeDisplayName;

        public bool HasFilePath => !string.IsNullOrWhiteSpace(this.Model.FilePath);

        public bool HasPackageName => !string.IsNullOrWhiteSpace(this.Model.PackageName);

        public bool HasErrorCode => !string.IsNullOrWhiteSpace(this.Model.ErrorCode);

        public string Title => this.PackageName ?? this.Source;

        public override string ToString()
        {
            return this.Message;
        }

        public string FormatAsString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}