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

        public DateTime DateTime => this.Model.DateTime;
        
        public string ErrorCode => this.Model.ErrorCode;

        public string Message => this.Model.Message;

        public string CompactMessage
        {
            get
            {
                string msg;
                if (this.Model.Message.IndexOf(System.Environment.NewLine, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    msg = this.Model.Message.Remove(this.Model.Message.IndexOf(System.Environment.NewLine, StringComparison.OrdinalIgnoreCase));
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
        
        public string Troubleshooting
        {
            get
            {
                if (this.Model.ErrorCode == null || !this.Model.ErrorCode.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return ExceptionGuard.Guard(() =>
                {
                    if (ErrorCodes.TryGetDescription(Convert.ToUInt32(this.Model.ErrorCode, 16), out var translated))
                    {
                        return translated;
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

        public string Level => this.Model.Level;

        public string Source => this.Model.Source;

        public string OpcodeDisplayName => this.Model.OpcodeDisplayName;

        public bool HasFilePath => !string.IsNullOrWhiteSpace(this.Model.FilePath);

        public bool HasPackageName => !string.IsNullOrWhiteSpace(this.Model.PackageName);

        public bool HasErrorCode => !string.IsNullOrWhiteSpace(this.Model.ErrorCode);

        public string Title => this.PackageName ?? this.Source;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}