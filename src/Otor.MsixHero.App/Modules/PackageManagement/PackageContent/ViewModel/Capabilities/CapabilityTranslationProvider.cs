using System.Text;
using System.Text.RegularExpressions;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Capabilities
{
    public class CapabilityTranslationProvider
    {
        public static string ToDisplayName(string name)
        {
            switch (name)
            {
                case "usb":
                    return Resources.Localization.Capability_USB;
                case "humaninterfacedevice":
                    return Resources.Localization.Capability_HID;
                case "pointOfService":
                    return Resources.Localization.Capability_POS;
                case "wiFiControl":
                    return Resources.Localization.Capability_WiFi;
                case "radios":
                    return Resources.Localization.Capability_RadioState;
                case "optical":
                    return Resources.Localization.Capability_OpticalDisc;
                case "activity":
                    return Resources.Localization.Capability_MotionActivity;
                case "serialcommunication":
                    return Resources.Localization.Capability_SerialCommunication;
                case "gazeInput":
                    return Resources.Localization.Capability_Gaze;
                case "lowLevel":
                    return Resources.Localization.Capability_LowLevel;
                case "documentsLibrary":
                    return Resources.Localization.Capability_Documents;
                case "appCaptureSettings":
                    return Resources.Localization.Capability_GameDvr;
                case "cellularDeviceControl":
                    return Resources.Localization.Capability_Cellular;
                case "cellularDeviceIdentity":
                    return Resources.Localization.Capability_CellularIdentity;
                case "cellularMessaging":
                    return Resources.Localization.Capability_CellularMsg;
                case "dualSimTiles":
                    return Resources.Localization.Capability_DualSim;
                case "enterpriseDeviceLockdown":
                    return Resources.Localization.Capability_EnterpriseSharedStorage;
                case "inputInjectionBrokered":
                    return Resources.Localization.Capability_SysInputInjection;
                case "inputObservation":
                    return Resources.Localization.Capability_ObserveInput;
                case "inputSuppression":
                    return Resources.Localization.Capability_SuppressInput;
                case "networkingVpnProvider":
                    return Resources.Localization.Capability_VPN;
                case "packageManagement":
                    return Resources.Localization.Capability_ManageOtherApps;
                case "packageQuery":
                    return Resources.Localization.Capability_GatherInfoOtherApps;
                case "screenDuplication":
                    return Resources.Localization.Capability_ScreenProjection;
                case "location":
                    return Resources.Localization.Capability_Location;
                case "webcam":
                    return Resources.Localization.Capability_Webcam;
                case "microphone":
                    return Resources.Localization.Capability_Microphone;
                case "userPrincipalName":
                    return Resources.Localization.Capability_UserPrincipalName;
                case "walletSystem":
                    return Resources.Localization.Capability_Wallet;
                case "locationHistory":
                    return Resources.Localization.Capability_LocationHistory;
                case "confirmAppClose":
                    return Resources.Localization.Capability_AppCloseConfirmation;
                case "phoneCallHistory":
                    return Resources.Localization.Capability_CallHistory;
                case "appointmentsSystem":
                    return Resources.Localization.Capability_SystemAppointment;
                case "chatSystem":
                    return Resources.Localization.Capability_SystemChat;
                case "contactsSystem":
                    return Resources.Localization.Capability_SystemContacts;
                case "email":
                    return Resources.Localization.Capability_Email;
                case "emailSystem":
                    return Resources.Localization.Capability_SystemEmail;
                case "phoneCallHistorySystem":
                    return Resources.Localization.Capability_SystemCallHistory;
                case "smsSend":
                    return Resources.Localization.Capability_Sms;
                case "userDataSystem":
                    return Resources.Localization.Capability_SystemAllUsers;
                case "previewStore":
                    return Resources.Localization.Capability_StorePreview;
                case "firstSignInSettings":
                    return Resources.Localization.Capability_FirstTimeSignIn;
                case "teamEditionExperience":
                    return Resources.Localization.Capability_WindowsTeamExperience;
                case "remotePassportAuthentication":
                    return Resources.Localization.Capability_RemoteUnlock;
                case "runFullTrust":
                    return Resources.Localization.Capability_FullTrust;
                case "allowElevation":
                    return Resources.Localization.Capability_Elevation;
                case "teamEditionDeviceCredential":
                    return Resources.Localization.Capability_WindowsTeamDeviceCreds;
                case "teamEditionView":
                    return Resources.Localization.Capability_WindowsTeamAppView;
                case "cameraProcessingExtension":
                    return Resources.Localization.Capability_CameraProcExt;
                case "musicLibrary":
                    return Resources.Localization.Capability_Music;
                case "picturesLibrary":
                    return Resources.Localization.Capability_Pictures;
                case "videosLibrary":
                    return Resources.Localization.Capability_Videos;
                case "removableStorage":
                    return Resources.Localization.Capability_RemovableStorage;
                case "internetClient":
                    return Resources.Localization.Capability_InternetClient;
                case "internetClientServer":
                    return Resources.Localization.Capability_InternetClientServer;
                case "privateNetworkClientServer":
                    return Resources.Localization.Capability_HomeNetwork;
                case "phoneCall":
                    return Resources.Localization.Capability_PhoneCalls;
                case "phoneCallHistoryPublic":
                    return Resources.Localization.Capability_PhoneCallHistory;
                case "voipCall":
                    return Resources.Localization.Capability_VoIP;
                case "objects3D":
                    return Resources.Localization.Capability_3d;
                case "blockedChatMessages":
                    return Resources.Localization.Capability_ReadBlockedMessages;
                case "lowLevelDevices":
                    return Resources.Localization.Capability_CustomDevices;
                case "systemManagement":
                    return Resources.Localization.Capability_IoT;
                case "backgroundMediaPlayback":
                    return Resources.Localization.Capability_BackgroundMedia;
                case "phoneLineTransportManagement":
                    return Resources.Localization.Capability_ManagePhoneLine;
                case "networkDataUsageManagement":
                    return Resources.Localization.Capability_DataUsageMgmt;
                case "smbios":
                    return Resources.Localization.Capability_Bios;
                case "broadFileSystemAccess":
                    return Resources.Localization.Capability_BroadFs;
                case "developmentModeNetwork":
                    return Resources.Localization.Capability_DevelopmentModeNetwork;
                case "oneProcessVoIP":
                    return Resources.Localization.Capability_ReserveVoIP;
                case "backgroundVoIP":
                    return Resources.Localization.Capability_AutoVoIP;
                case "enterpriseCloudSSO":
                    return Resources.Localization.Capability_EnterpriseSSO;
                case "uiAutomation":
                    return Resources.Localization.Capability_UIAutomation;
                case "userSystemId":
                    return Resources.Localization.Capability_UserSystemID;
                case "previewPenWorkspace":
                    return Resources.Localization.Capability_PenWorkspace;
                case "appLicensing":
                    return Resources.Localization.Capability_AppLicensing;
                case "oemDeployment":
                    return Resources.Localization.Capability_OemDeploy;
                case "userSigninSupport":
                    return Resources.Localization.Capability_UserSignIn;
                case "inputForegroundObservation":
                    return Resources.Localization.Capability_ForegroundObservation;
                case "interopServices":
                    return Resources.Localization.Capability_DriverAccess;
                case "accessoryManager":
                    return Resources.Localization.Capability_AccessoryManagement;
                case "cortanaSpeechAccessory":
                    return Resources.Localization.Capability_SpeechRecognition;
                case "xboxAccessoryManagement":
                    return Resources.Localization.Capability_XboxAccessory;
                case "gameList":
                    return Resources.Localization.Capability_GamesList;
                case "packagePolicySystem":
                    return Resources.Localization.Capability_PackagePolicyControl;
                case "deviceManagementDmAccount":
                    return Resources.Localization.Capability_ProvisionConfigureOmaDm;
                case "deviceManagementFoundation":
                    return Resources.Localization.Capability_MdmCspInf;
                case "deviceManagementWapSecurityPolicies":
                    return Resources.Localization.Capability_ConfigureWAP;
                case "deviceManagementEmailAccount":
                    return Resources.Localization.Capability_ManageEmailAccount;
                case "extendedBackgroundTaskTime":
                    return Resources.Localization.Capability_ExtendedBgTaskTime;
                case "extendedExecutionBackgroundAudio":
                    return Resources.Localization.Capability_ExtendedExecBgAudio;
                case "slapiQueryLicenseValue":
                    return Resources.Localization.Capability_SoftwareLicensing;
                case "inProcessMediaExtension":
                    return Resources.Localization.Capability_InProcMediaExt;
                case "hidTelephony":
                    return Resources.Localization.Capability_HidTelephony;
                default:

                    var regex = Regex.Matches(name, "[A-Z](?![A-Z])");
                    var sb = new StringBuilder();

                    var previous = name.Length;
                    for (var i = regex.Count - 1; i >= 0; i--)
                    {
                        sb.Insert(0, name.Substring(regex[i].Index, previous - regex[i].Index));
                        previous = regex[i].Index;

                        if (previous > 0)
                        {
                            sb.Insert(0, " ");
                        }
                    }

                    if (previous > 0)
                    {
                        sb.Insert(0, name.Substring(0, previous));
                    }

                    var trimmed = sb.ToString().Trim();
                    if (char.IsUpper(trimmed[0]))
                    {
                        return trimmed;
                    }

                    return trimmed.Substring(0, 1).ToUpperInvariant() + trimmed.Substring(1);
            }
        }
    }
}