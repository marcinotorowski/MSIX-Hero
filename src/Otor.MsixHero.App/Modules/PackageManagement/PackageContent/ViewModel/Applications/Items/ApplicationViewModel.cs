using System;
using System.Linq;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Common.Enums;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Applications.Items
{
    public class ApplicationViewModel : NotifyPropertyChanged
    {
        private readonly AppxApplication _model;
        private readonly AppxPackage _package;
        private bool _isExpanded;

        public ApplicationViewModel(AppxApplication appxApplication, AppxPackage package)
        {
            this._model = appxApplication;
            this._package = package;
            this.Type = PackageTypeConverter.GetPackageTypeFrom(_model.EntryPoint, _model.Executable, _model.StartPage, _package.IsFramework, _model.HostId);
            this.Alias = _model.ExecutionAlias?.Any() == true ? string.Join(", ", _model.ExecutionAlias.Distinct(StringComparer.OrdinalIgnoreCase)) : null;
        }

        public bool Visible => _model.Visible;

        public bool IsExpanded
        {
            get => this._isExpanded;
            set => this.SetField(ref this._isExpanded, value);
        }

        public string Alias { get; }

        public string DisplayName => _model.DisplayName;

        public byte[] Image => _model.Logo;

        public string Id => _model.Id;

        public string TileColor => string.Equals(_model.BackgroundColor, "transparent", StringComparison.OrdinalIgnoreCase) ? null : _model.BackgroundColor;

        public string Target
        {
            get
            {
                switch (PackageTypeConverter.GetPackageTypeFrom(_model.EntryPoint, _model.Executable, _model.StartPage, _package.IsFramework))
                {
                    case MsixApplicationType.Win32:
                    case MsixApplicationType.Win32Psf:
                    case MsixApplicationType.Win32AiStub:
                    case MsixApplicationType.MsixHelper:
                        return _model.Executable;
                    case MsixApplicationType.Web:
                        return _model.StartPage;
                    default:
                        if (string.IsNullOrEmpty(_model.EntryPoint))
                        {
                            return _model.Executable;
                        }

                        return _model.Executable;
                }
            }
        }

        public string ActualTarget
        {
            get
            {
                if (_model.Proxy != null)
                {
                    if (_model.Proxy?.Arguments == null)
                    {
                        return _model.Proxy?.Executable ?? _model.Executable;
                    }

                    return _model.Proxy.Executable + " " + _model.Proxy.Arguments;
                }

                return string.IsNullOrEmpty(_model.EntryPoint) ? _model.Executable : _model.EntryPoint;
            }
        }

        public string OriginalTarget
        {
            get
            {
                switch (Type)
                {
                    case MsixApplicationType.Win32Psf:
                    case MsixApplicationType.Win32AiStub:
                        if (_model.Proxy?.Executable == null)
                        {
                            return null;
                        }

                        return _model.Executable;
                }

                return null;
            }
        }

        public string WorkingDirectory
        {
            get
            {
                if (_model.Proxy == null)
                {
                    return null;
                }

                return _model.Proxy.WorkingDirectory;
            }
        }

        public string Arguments => _model.Proxy?.Arguments ?? _model.Parameters;

        public string HostId => _model.HostId;

        public string EntryPoint => Type == MsixApplicationType.Uwp ? _model.EntryPoint : null;

        public bool HasEntryPoint => Type == MsixApplicationType.Uwp;

        public bool HasHostId => !string.IsNullOrEmpty(HostId);

        public bool HasOriginalTarget => !string.IsNullOrEmpty(OriginalTarget);

        public bool HasWorkingDirectory => !string.IsNullOrEmpty(WorkingDirectory);

        public bool HasArguments => !string.IsNullOrEmpty(Arguments);

        public bool HasAlias => !string.IsNullOrEmpty(Alias);

        public MsixApplicationType Type { get; }

        public string DisplayedType => PackageTypeConverter.GetPackageTypeStringFrom(_model.EntryPoint, _model.Executable, _model.StartPage, _package.IsFramework, _model.HostId, PackageTypeDisplay.Long);
    }
}
