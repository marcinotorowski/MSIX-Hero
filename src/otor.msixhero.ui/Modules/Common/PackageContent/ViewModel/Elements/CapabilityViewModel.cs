using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;

namespace otor.msixhero.ui.Modules.Common.PackageContent.ViewModel.Elements
{
    public class CapabilitiesViewModel
    {
        public CapabilitiesViewModel(IEnumerable<AppxCapability> capabilities)
        {
            this.Count = 0;
            foreach (var c in capabilities.GroupBy(c => c.Type))
            {
                switch (c.Key)
                {
                    case CapabilityType.General:
                        this.General = new List<CapabilityViewModel>(c.Select(cap => new CapabilityViewModel(cap)));
                        this.Count += this.General.Count;
                        break;
                    case CapabilityType.Restricted:
                        this.Restricted = new List<CapabilityViewModel>(c.Select(cap => new CapabilityViewModel(cap)));
                        this.Count += this.Restricted.Count;
                        break;
                    case CapabilityType.Device:
                        this.Device = new List<CapabilityViewModel>(c.Select(cap => new CapabilityViewModel(cap)));
                        this.Count += this.Device.Count;
                        break;
                    default:
                        this.Custom = new List<CapabilityViewModel>(c.Select(cap => new CapabilityViewModel(cap)));
                        this.Count += this.Custom.Count;
                        break;
                }
            }
        }

        public int Count { get; }

        public IReadOnlyCollection<CapabilityViewModel> General { get; }

        public IReadOnlyCollection<CapabilityViewModel> Restricted { get; }

        public IReadOnlyCollection<CapabilityViewModel> Device { get; }

        public IReadOnlyCollection<CapabilityViewModel> Custom { get; }

        public bool HasGeneral => this.General?.Any() == true;

        public bool HasDevice => this.Device?.Any() == true;

        public bool HasCustom => this.Custom?.Any() == true;

        public bool HasRestricted => this.Restricted?.Any() == true;
    }

    public class CapabilityViewModel
    {
        private static readonly Lazy<System.Windows.Media.Geometry> DefaultIcon = new Lazy<System.Windows.Media.Geometry>(() => System.Windows.Media.Geometry.Parse("M 16 2.71875 L 12.5625 5.46875 L 8.9375 5.3125 L 8.125 5.25 L 7.9375 6.03125 L 7.03125 9.5 L 4.03125 11.46875 L 3.375 11.90625 L 3.65625 12.65625 L 4.9375 16 L 3.65625 19.34375 L 3.375 20.09375 L 4.03125 20.53125 L 7.03125 22.5 L 7.9375 25.96875 L 8.125 26.75 L 8.9375 26.6875 L 12.5625 26.53125 L 16 29.28125 L 19.4375 26.53125 L 23.0625 26.6875 L 23.875 26.75 L 24.0625 25.96875 L 24.96875 22.5 L 27.96875 20.53125 L 28.625 20.09375 L 28.34375 19.34375 L 27.0625 16 L 28.34375 12.65625 L 28.625 11.90625 L 27.96875 11.46875 L 24.96875 9.5 L 24.0625 6.03125 L 23.875 5.25 L 23.0625 5.3125 L 19.4375 5.46875 Z M 16 5.28125 L 18.46875 7.28125 L 18.78125 7.53125 L 19.15625 7.5 L 22.34375 7.34375 L 23.125 10.34375 L 23.21875 10.71875 L 23.53125 10.9375 L 26.1875 12.6875 L 25.0625 15.65625 L 24.9375 16 L 25.0625 16.34375 L 26.1875 19.3125 L 23.53125 21.0625 L 23.21875 21.28125 L 23.125 21.65625 L 22.34375 24.65625 L 19.15625 24.5 L 18.78125 24.46875 L 18.46875 24.71875 L 16 26.71875 L 13.53125 24.71875 L 13.21875 24.46875 L 12.84375 24.5 L 9.65625 24.65625 L 8.875 21.65625 L 8.78125 21.28125 L 8.46875 21.0625 L 5.8125 19.3125 L 6.9375 16.34375 L 7.0625 16 L 6.9375 15.65625 L 5.8125 12.6875 L 8.46875 10.9375 L 8.78125 10.71875 L 8.875 10.34375 L 9.65625 7.34375 L 12.84375 7.5 L 13.21875 7.53125 L 13.53125 7.28125 Z M 21.28125 12.28125 L 15 18.5625 L 11.71875 15.28125 L 10.28125 16.71875 L 14.28125 20.71875 L 15 21.40625 L 15.71875 20.71875 L 22.71875 13.71875 Z"));

        public CapabilityViewModel(AppxCapability capability)
        {
            this.Name = capability.Name;
            this.Type = capability.Type;
        }

        public string Name { get; }

        public string DisplayName
        {
            get => GetCapabilityDisplayNameFromEntity(this.Type, this.Name);
        }

        public Geometry VectorIcon
        {
            get => GetCapabilityVectorPathFromEntity(this.Type, this.Name);
        }

        public CapabilityType Type { get; }

        private static Geometry GetCapabilityVectorPathFromEntity(CapabilityType type, string name)
        {
            switch (name)
            {
                case "elevation":
                    return Geometry.Parse("M 16 4 C 13.75 4 12.234375 4.886719 10.875 5.625 C 9.515625 6.363281 8.28125 7 6 7 L 5 7 L 5 8 C 5 15.71875 7.609375 20.742188 10.25 23.78125 C 12.890625 26.820313 15.625 27.9375 15.625 27.9375 L 16 28.0625 L 16.375 27.9375 C 16.375 27.9375 19.109375 26.84375 21.75 23.8125 C 24.390625 20.78125 27 15.746094 27 8 L 27 7 L 26 7 C 23.730469 7 22.484375 6.363281 21.125 5.625 C 19.765625 4.886719 18.25 4 16 4 Z M 16 6 C 17.75 6 18.753906 6.613281 20.15625 7.375 C 21.339844 8.019531 22.910156 8.636719 24.9375 8.84375 C 24.746094 15.609375 22.507813 19.910156 20.25 22.5 C 18.203125 24.847656 16.484375 25.628906 16 25.84375 C 15.511719 25.625 13.796875 24.824219 11.75 22.46875 C 9.492188 19.871094 7.253906 15.578125 7.0625 8.84375 C 9.097656 8.636719 10.660156 8.019531 11.84375 7.375 C 13.246094 6.613281 14.25 6 16 6 Z M 11 10 L 11 19 L 21 19 L 21 10 Z M 13 13 L 19 13 L 19 17 L 13 17 Z");
                case "runFullTrust":
                    return Geometry.Parse("M 16 4 C 9.3844239 4 4 9.3844287 4 16 C 4 22.615571 9.3844239 28 16 28 C 22.615576 28 28 22.615571 28 16 C 28 9.3844287 22.615576 4 16 4 z M 16 6 C 21.534697 6 26 10.465307 26 16 C 26 21.534693 21.534697 26 16 26 C 10.465303 26 6 21.534693 6 16 C 6 10.465307 10.465303 6 16 6 z M 20.949219 12 L 14.699219 18.25 L 11.449219 15 L 10.050781 16.400391 L 14.699219 21.050781 L 22.349609 13.400391 L 20.949219 12 z");
            }

            return DefaultIcon.Value;
        }

        private static string GetCapabilityDisplayNameFromEntity(CapabilityType type, string name)
        {
            switch (name)
            {
                case "location":
                    return "Location";
                case "microphone":
                    return "Microphone";
                case "proximity":
                    return "Proximity";
                case "webcam":
                    return "Webcam";
                case "usb":
                    return "USB";
                case "humaninterfacedevice":
                    return "Human interface device (HID)";
                case "pointOfService":
                    return "Point of Service (POS)";
                case "bluetooth":
                    return "Bluetooth";
                case "wiFiControl":
                    return "Wi-Fi networking";
                case "radios":
                    return "Radio state";
                case "optical":
                    return "Optical disc";
                case "activity":
                    return "Motion activity";
                case "serialcommunication":
                    return "Serial communication";
                case "gazeInput":
                    return "Eye tracker";
                case "lowLevel":
                    return "GPIO, I2C, SPI, and PWM";
                case "enterpriseAuthentication":
                    return "Enterprise authentication";
                case "enterpriseDataPolicy":
                    return "Enterprise data policy";
                case "sharedUserCertificates":
                    return "Add and access certificates in Shared User store";
                case "documentsLibrary":
                    return "Access to user's Documents library";
                case "appCaptureSettings":
                    return "Game DVR Settings";
                case "cellularDeviceControl":
                    return "Control over cellular device";
                case "cellularDeviceIdentity":
                    return "Access celullar identification data";
                case "cellularMessaging":
                    return "Make use of SMS and RCS";
                case "deviceUnlock":
                    return "Device unlock";
                case "dualSimTiles":
                    return "Dual SIM Tiles";
                case "enterpriseDeviceLockdown":
                    return "Enterprise Shared Storage";
                case "inputInjectionBrokered":
                    return "System Input Injection";
                case "inputObservation":
                    return "Observe Input";
                case "inputSuppression":
                    return "Suppress Input";
                case "networkingVpnProvider":
                    return "Access to VPN features";
                case "packageManagement":
                    return "Manage other apps";
                case "packageQuery":
                    return "Gather information about other apps";
                case "screenDuplication":
                    return "Project the screen on another device";
                case "userPrincipalName":
                    return "Access user principal name";
                case "walletSystem":
                    return "Access to the stored wallet cards";
                case "locationHistory":
                    return "Access location history";
                case "confirmAppClose":
                    return "App can close itself and delay closing";
                case "phoneCallHistory":
                    return "Read and delete entries in call history";
                case "appointmentsSystem":
                    return "System Level Appointment Access";
                case "chatSystem":
                    return "System Level Chat Message Access";
                case "contactsSystem":
                    return "System Level Contact Access";
                case "email":
                    return "Email Access";
                case "emailSystem":
                    return "System Level Email Access";
                case "phoneCallHistorySystem":
                    return "System Level Call History Access";
                case "smsSend":
                    return "Send Text Messages";
                case "userDataSystem":
                    return "System Level Access to All User Data";
                case "previewStore":
                    return "Store Preview Feature";
                case "firstSignInSettings":
                    return "First-Time Sign-in Settings";
                case "teamEditionExperience":
                    return "Windows Team Experience";
                case "remotePassportAuthentication":
                    return "Remote Unlock";
                case "runFullTrust":
                    return "Full Trust Permission Level";
                case "allowElevation":
                    return "Elevation";
                case "teamEditionDeviceCredential":
                    return "Windows Team Device Credentials";
                case "teamEditionView":
                    return "Windows Team Application View";
                case "cameraProcessingExtension":
                    return "Camera Processing Extension";
                case "musicLibrary":
                    return "Music";
                case "picturesLibrary":
                    return "Pictures";
                case "videosLibrary":
                    return "Videos";
                case "removableStorage":
                    return "Removable storage";
                case "internetClient":
                    return "Internet and public networks (client)";
                case "internetClientServer":
                    return "Internet and public networks (client + server)";
                case "privateNetworkClientServer":
                    return "Homes and work networks";
                case "appointments":
                    return "Appointments";
                case "contacts":
                    return "Contacts";
                case "codeGeneration":
                    return "Code generation";
                case "allJoyn":
                    return "AllJoyn";
                case "phoneCall":
                    return "Phone calls";
                case "phoneCallHistoryPublic":
                    return "Phone call history";
                case "recordedCallsFolder":
                    return "Recorded calls folder";
                case "userAccountInformation":
                    return "User account information";
                case "voipCall":
                    return "VoIP calling";
                case "objects3D":
                    return "3D Objects";
                case "blockedChatMessages":
                    return "Read blocked messages";
                case "lowLevelDevices":
                    return "Custom devices";
                case "systemManagement":
                    return "IoT system administration";
                case "backgroundMediaPlayback":
                    return "Background media playback";
                case "remoteSystem":
                    return "Remote system";
                case "Spatial perception":
                    return "spatialPerception  yy";
                case "globalMediaControl":
                    return "Global media control";
            }

            return name;
        }
    }
}
