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

using System;
using Newtonsoft.Json;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;

namespace Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels
{
    public class LogViewModel : NotifyPropertyChanged
    {
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