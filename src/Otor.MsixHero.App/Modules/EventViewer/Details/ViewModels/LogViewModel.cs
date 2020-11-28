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