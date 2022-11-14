using Otor.MsixHero.App.Modules.Common.CertificateSelector.ViewModel;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Signing.Testing;
using Otor.MsixHero.Appx.Signing.TimeStamping;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Cryptography;
using Otor.MsixHero.Infrastructure.Services;
using System;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Signing.ViewModel
{
    public class SignProfileViewModel : CertificateSelectorViewModel
    {
        private readonly SigningProfile _profile;
        private bool _isEditing;

        public SignProfileViewModel(
            ISigningTestService signTestService,
            IInteractionService interactionService,
            IUacElevation uacElevation,
            SigningConfiguration signConfiguration,
            SigningProfile profile,
            ITimeStampFeed timeStampFeed) : base(signTestService, interactionService, uacElevation, signConfiguration, profile, timeStampFeed)
        {
            _profile = profile;
            DisplayName = new ValidatedChangeableProperty<string>("Profile name", profile.Name, ValidatorFactory.ValidateNotEmptyField("Display name"));
            IsDefault = new ChangeableProperty<bool>(profile.IsDefault);
            AddChildren(DisplayName, IsDefault);
        }

        public ValidatedChangeableProperty<string> DisplayName { get; }

        public ChangeableProperty<bool> IsDefault { get; }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetField(ref _isEditing, value);
        }

        public SigningProfile ToSigningProfile()
        {
            var newProfile = new SigningProfile
            {
                DeviceGuard = _profile.DeviceGuard == null ? null : new DeviceGuardConfiguration
                {
                    EncodedAccessToken = _profile.DeviceGuard?.EncodedAccessToken,
                    EncodedRefreshToken = _profile.DeviceGuard?.EncodedRefreshToken,
                    Subject = _profile.DeviceGuard?.Subject,
                },
                EncodedPassword = _profile.EncodedPassword,
                Name = _profile.Name,
                Source = _profile.Source,
                PfxPath = _profile.PfxPath,
                Thumbprint = _profile.Thumbprint,
                TimeStampServer = _profile.TimeStampServer,
                IsDefault = _profile.IsDefault
            };

            if (IsDefault.IsTouched)
            {
                newProfile.IsDefault = IsDefault.CurrentValue;
            }

            if (Store.CurrentValue == CertificateSource.Pfx)
            {
                if (Store.IsTouched || PfxPath.IsTouched)
                {
                    newProfile.PfxPath.Resolved = PfxPath.CurrentValue;
                }

                if (Store.IsTouched || Password.IsTouched)
                {
                    var encoder = new Crypto();
                    newProfile.EncodedPassword = encoder.Protect(Password.CurrentValue);
                }
            }
            else
            {
                newProfile.EncodedPassword = null;
                newProfile.PfxPath = null;
            }

            if (Store.CurrentValue == CertificateSource.Personal)
            {
                if (Store.IsTouched || SelectedPersonalCertificate.IsTouched)
                {
                    newProfile.Thumbprint = SelectedPersonalCertificate.CurrentValue?.Model?.Thumbprint;
                }
            }
            else
            {
                newProfile.Thumbprint = null;
            }

            if (Store.CurrentValue == CertificateSource.DeviceGuard)
            {
                if (Store.IsTouched || DeviceGuard.IsTouched)
                {
                    newProfile.DeviceGuard = DeviceGuard.CurrentValue;
                }
            }
            else
            {
                newProfile.DeviceGuard = null;
            }


            string timeStampUrl;
            switch (TimeStampSelectionMode.CurrentValue)
            {
                case Common.CertificateSelector.ViewModel.TimeStampSelectionMode.None:
                    timeStampUrl = null;
                    break;
                case Common.CertificateSelector.ViewModel.TimeStampSelectionMode.Auto:
                    timeStampUrl = "auto";
                    break;
                case Common.CertificateSelector.ViewModel.TimeStampSelectionMode.Url:
                    timeStampUrl = TimeStamp.CurrentValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            newProfile.TimeStampServer = timeStampUrl;

            return newProfile;
        }
    }
}
