using System;
using System.Collections.Generic;

namespace Otor.MsixHero.Appx.Diagnostic.Logging
{
    public class ErrorCodes
    {
        private readonly Lazy<IDictionary<uint, ErrorCode>> _codes = new(GenerateErrorCodes);

        public bool TryGetTitle(uint errorCode, out string message)
        {
            if (_codes.Value.TryGetValue(errorCode, out var definition))
            {
                message = definition.Title;
                return true;
            }

            message = null;
            return false;
        }

        public bool TryGetDescription(uint errorCode, out string message)
        {
            if (_codes.Value.TryGetValue(errorCode, out var definition))
            {
                message = definition.Description;
                return true;
            }

            message = null;
            return false;
        }

        public bool TryGetCode(uint errorCode, out string message)
        {
            if (_codes.Value.TryGetValue(errorCode, out var definition))
            {
                message = definition.Code;
                return true;
            }

            message = null;
            return false;
        }

        private static IDictionary<uint, ErrorCode> GenerateErrorCodes()
        {
            var dict = new Dictionary<uint, ErrorCode>();

            var factory = new ErrorCodesFactory();
            foreach (var item in factory.GetKnownErrors())
            {
                dict[item.NumericCode] = item;
            }

            return dict;
        }

        public struct ErrorCode
        {
            public ErrorCode(string code, string title, uint numericCode, string description)
            {
                this.Code = code;
                this.Title = title;
                this.NumericCode = numericCode;
                this.Description = description;
            }

            public string Code { get; }
            
            public string Title { get; }
            
            public uint NumericCode { get; }

            public string Description { get; }
        }
    }

    public class ErrorCodesFactory
    {
        public IEnumerable<ErrorCodes.ErrorCode> GetKnownErrors()
        {
            // ReSharper disable StringLiteralTypo
            yield return new ErrorCodes.ErrorCode("E_FILENOTFOUND", "File not found", 0x80070002, "File or path is not found. This can occur during a COM typelib validation requires that the path for the directory actually exist within your MSIX package.");
            yield return new ErrorCodes.ErrorCode("ERROR_BAD_FORMAT", "Wrong package format", 0x8007000B, "The package isn't correctly formatted and needs to be re-built or re-signed.\r\nYou may get this error if there is a mismatch between the signing certificate subject name and the AppxManifest.xml publisher name.\r\nSee How to sign an app package using SignTool.");
            yield return new ErrorCodes.ErrorCode("E_INVALIDARG", "Invalid arguments", 0x80070057, "One or more arguments are not valid.If you check the AppXDeployment - Server event log and see the following event: \"While installing the package, the system failed to register the windows.repositoryExtension extension due to the following error: The parameter is incorrect.\"\r\nYou may get this error if the manifest elements DisplayName or Description contain characters disallowed by Windows firewall such as |, due to which Windows fails to create the AppContainer profile for the package. Please remove these characters from the manifest and try installing the package.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_OPEN_PACKAGE_FAILED", "Package could not be opened", 0x80073CF0, "The package couldn't be opened.\r\nPossible causes:\r\n\r\nThe package is unsigned.\\r\\nThe publisher name doesn't match the signing certificate subject.\r\nThe file:// prefix is missing or the package couldn't be found at the specified location.\r\nFor more information, check the AppxPackagingOM event log.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_PACKAGE_NOT_FOUND", "Package not found", 0x80073CF1, "The package couldn't be found.\r\nYou may get this error while removing a package that isn't installed for the current user.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_INVALID_PACKAGE", "Invalid package data", 0x80073CF2, null);
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_RESOLVE_DEPENDENCY_FAILED", "Failed update, dependency, or conflict validation.", 0x80073CF3, "The package failed update, dependency, or conflict validation. Possible causes:\r\n\r\nThe incoming package conflicts with an installed package.A specified package dependency can't be found.\r\nThe package doesn't support the correct processor architecture.\r\n\r\nFor more information, check the AppXDeployment-Server event log.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_OUT_OF_DISK_SPACE", "Not enough disk space", 0x80073CF4, "There isn't enough disk space on your computer. Free some space and try again.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_NETWORK_FAILURE", "Download failed", 0x80073CF5, "The package can't be downloaded.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_REGISTRATION_FAILURE", "Registration failed", 0x80073CF6, "The package can't be registered.\r\nFor more information, check the AppXDeployment-Server event log.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_DEREGISTRATION_EFAILURE", "Deregistration failed", 0x80073CF7, "The package can't be unregistered.\r\nYou may get this error while removing a package.\r\nFor more information, check the AppXDeployment-Server event log.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_CANCEL", "Installation cancelled by user", 0x80073CF8, "The user canceled the install request.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_FAILED", "Installation failed", 0x80073CF9, "Package install failed. Contact the software vendor.\r\nFor more information, check the AppXDeployment-Server event log.");
            yield return new ErrorCodes.ErrorCode("ERROR_REMOVE_FAILED", "Removal failed", 0x80073CFA, "Package removal failed.\r\nYou may get this error for failures that occur during package uninstall.\r\nFor more information, see RemovePackageAsync.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGE_ALREADY_EXISTS", "Package already exists", 0x80073CFB, "The provided package is already installed, and reinstallation of the package is blocked.\r\nYou may get this error if installing a package that is not bitwise identical to the package that is already installed. Note that the digital signature is also part of the package. Hence if a package is rebuilt or resigned, it is no longer bitwise identical to the previously installed package. Two possible options to fix this error are: (1) Increment the version number of the app, then rebuild and resign the package (2) Remove the old package for every user on the system before installing the new package.");
            yield return new ErrorCodes.ErrorCode("ERROR_NEEDS_REMEDIATION", "App cannot be started", 0x80073CFC, "The app can't be started. Try reinstalling the app.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_PREREQUISITE_FAILED", "Prerequisite failed", 0x80073CFD, "A specified install prerequisite couldn't be satisfied.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGE_REPOSITORY_CORRUPTED", "Corrupted repository", 0x80073CFE, "The package repository is corrupted.\r\nYou may get this error if the folder referenced by this registry key doesn't exist or is corrupted: \r\nHKLM\\Software\\Microsoft\\Windows</strong>\r\nCurrentVersion\\Appx\\PackageRepositoryRoot\r\n\r\nTo recover from this state, refresh your PC.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_POLICY_FAILURE", "Policy failure", 0x80073CFF, "To install this app, you need a developer license or a sideloading-enabled system.\r\nYou may get this error if the package doesn't meet one of the following requirements:\r\nThe app is deployed using F5 in Visual Studio on a computer with a Windows developer license.\r\nThe package is signed with a Microsoft signature and deployed as part of Windows or from the Microsoft Store.\r\nThe package is signed with a trusted signature and installed on a computer with a developer license, a domain-joined computer with the AllowAllTrustedApps policy enabled, or a computer with a Windows Sideloading license with the AllowAllTrustedApps policy enabled.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGE_UPDATING", "Update in progress", 0x80073D00, "The app can't be started because it's currently updating.");
            yield return new ErrorCodes.ErrorCode("ERROR_DEPLOYMENT_BLOCKED_BY_POLICY", "Deployment was blocked", 0x80073D01, "The package deployment operation is blocked by policy. Contact your system administrator. Possible causes:\r\nPackage deployment is blocked by Application Control Policies.\r\nPackage deployment is blocked by the \"Allow deployment operations in special profiles\"\\ policy.\r\n\r\nOne of the possible reasons is a need for a roaming profile. For information about setting up Roaming User Profiles on user accounts, see Deploy Roaming User Profiles. If there are no policies configured on your system and you still see this error, perhaps you are logged in with a temporary profile. Log out and log in again, then try the operation again.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGES_IN_USE", "Package in use", 0x80073D02, "The package couldn't be installed because resources it modifies are currently in use.");
            yield return new ErrorCodes.ErrorCode("ERROR_RECOVERY_FILE_CORRUPT", "Corrupted recovery file", 0x80073D03, "The package couldn't be recovered because data that's necessary for recovery is corrupted.");
            yield return new ErrorCodes.ErrorCode("ERROR_INVALID_STAGED_SIGNATURE", "Invalid signature", 0x80073D04, "The signature isn't valid. To register in developer mode, AppxSignature.p7x and AppxBlockMap.xml must be valid or shouldn't be present. If you are a developer using F5 with Visual Studio, ensure that your built project directory doesn't contain signature or block map files from previous versions of the package.");
            yield return new ErrorCodes.ErrorCode("ERROR_DELETING_EXISTING_APPLICATIONDATA_STORE_FAILED", "Failed removal of application data", 0x80073D05, "An error occurred while deleting the package's previously existing application data.\r\n You can get this error if the simulator is running. Close the simulator. You can also get this error if there are files open in the app data (for example, if you have a log file open in a text editor).");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_PACKAGE_DOWNGRADE", "Detected package downgrade", 0x80073D06, "The package couldn't be installed because a higher version of this package is already installed.");
            yield return new ErrorCodes.ErrorCode("ERROR_SYSTEM_NEEDS_REMEDIATION", "Corrupted system files", 0x80073D07, "An error in a system binary was detected. To fix the problem, try refreshing the PC.");
            yield return new ErrorCodes.ErrorCode("ERROR_APPX_INTEGRITY_FAILURE_EXTERNAL", "Corrupted software files", 0x80073D08, "A corrupted non-Windows binary was detected on the system.");
            yield return new ErrorCodes.ErrorCode("ERROR_RESILIENCY_FILE_CORRUPT", "Corrupted recovery data", 0x80073D09, "The operation couldn't be resumed because data that's necessary for recovery is corrupted.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_FIREWALL_SERVICE_NOT_RUNNING", "Windows Firewall is disabled", 0x80073D0A, "The package couldn't be installed because the Windows Firewall service isn't running. Enable the Windows Firewall service and try again.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGE_MOVE_FAILED", "Moving failed", 0x80073D0B, "The package move operation failed.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_VOLUME_NOT_EMPTY", "Volume is not empty", 0x80073D0C, "The deployment operation failed because the volume is not empty.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_VOLUME_OFFLINE", "Volume is offline", 0x80073D0D, "The deployment operation failed because the volume is offline. For a package update, the volume refers to the installed volume of all package versions.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_VOLUME_CORRUPT", "Corrupted volume", 0x80073D0E, "The deployment operation failed because the specified volume is corrupt.");
            yield return new ErrorCodes.ErrorCode("ERROR_NEEDS_REGISTRATION", "Application is not registered", 0x80073D0F, "The deployment operation failed because the specified application needs to be registered first.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_WRONG_PROCESSOR_ARCHITECTURE", "Mismatched processor architecture", 0x80073D10, "The deployment operation failed because the package targets the wrong processor architecture.");
            yield return new ErrorCodes.ErrorCode("ERROR_DEV_SIDELOAD_LIMIT_EXCEEDED", "Maximum number of sideloaded installations reached", 0x80073D11, "You have reached the maximum number of developer sideloaded packages allowed on this device. Please uninstall a sideloaded package and try again.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_OPTIONAL_PACKAGE_REQUIRES_MAIN_PACKAGE", "Missing main package", 0x80073D12, "A main app package is required to install this optional package. Install the main package first and try again.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGE_NOT_SUPPORTED_ON_FILESYSTEM", "Package type not supported", 0x80073D13, "This app package type is not supported on this filesystem.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGE_MOVE_BLOCKED_BY_STREAMING", "Streaming has not yet finished", 0x80073D14, "Package move operation is blocked until the application has finished streaming.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_OPTIONAL_PACKAGE_APPLICATIONID_NOT_UNIQUE", "Duplicated application identifier", 0x80073D15, "A main or another optional app package has the same application ID as this optional package. Change the application ID for the optional package to avoid conflicts.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGE_STAGING_ONHOLD", "Staging on hold", 0x80073D16, "This staging session has been held to allow another staging operation to be prioritized.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_INVALID_RELATED_SET_UPDATE", "Invalid related set", 0x80073D17, "A related set cannot be updated because the updated set is invalid. All packages in the related set must be updated at the same time.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_OPTIONAL_PACKAGE_REQUIRES_MAIN_PACKAGE_FULLTRUST_CAPABILITY", "Missing full trust capability", 0x80073D18, "An optional package with a FullTrust entry point requires the main package to have the runFullTrust capability.");
            yield return new ErrorCodes.ErrorCode("ERROR_DEPLOYMENT_BLOCKED_BY_USER_LOG_OFF", "User is not logged in", 0x80073D19, "An error occurred because a user was logged off.");
            yield return new ErrorCodes.ErrorCode("ERROR_PROVISION_OPTIONAL_PACKAGE_REQUIRES_MAIN_PACKAGE_PROVISIONED", "Main app has not been installed", 0x80073D1A, "An optional package provision requires the dependency main package to also be provisioned.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGES_REPUTATION_CHECK_FAILED", "Failed SmartScreen check", 0x80073D1B, "The packages failed the SmartScreen reputation check.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGES_REPUTATION_CHECK_TIMEDOUT", "SmartScreen check timed out", 0x80073D1C, "The SmartScreen reputation check operation timed out.");
            yield return new ErrorCodes.ErrorCode("ERROR_DEPLOYMENT_OPTION_NOT_SUPPORTED", "Option not supported", 0x80073D1D, "The current deployment option is not supported.");
            yield return new ErrorCodes.ErrorCode("ERROR_APPINSTALLER_ACTIVATION_BLOCKED", "Blocked activation", 0x80073D1E, "Activation is blocked due to the .appinstaller update settings for this app.");
            yield return new ErrorCodes.ErrorCode("ERROR_REGISTRATION_FROM_REMOTE_DRIVE_NOT_SUPPORTED", "Remote drives not supported", 0x80073D1F, "Remote drives are not supported. Use \\server\\share to register a remote package.");
            yield return new ErrorCodes.ErrorCode("ERROR_APPX_RAW_DATA_WRITE_FAILED", "Writing to disk failed", 0x80073D20, "Failed to process and write downloaded package data to disk.");
            yield return new ErrorCodes.ErrorCode("ERROR_DEPLOYMENT_BLOCKED_BY_VOLUME_POLICY_PACKAGE", "Blocked by policy", 0x80073D21, "The deployment operation was blocked due to a per-package-family policy restricting deployments on a non-system volume. Per policy, this app must be installed to the system drive, but that's not set as the default. In Storage settings, make the system drive the default location to save new content, then retry the install.");
            yield return new ErrorCodes.ErrorCode("ERROR_DEPLOYMENT_BLOCKED_BY_VOLUME_POLICY_MACHINE", "Blocked by policy", 0x80073D22, "The deployment operation was blocked due to a machine-wide policy restricting deployments on a non-system volume. Per policy, this app must be installed to the system drive, but that's not set as the default. In Storage settings, make the system drive the default location to save new content, then retry the install.");
            yield return new ErrorCodes.ErrorCode("ERROR_DEPLOYMENT_BLOCKED_BY_PROFILE_POLICY", "Blocked by policy", 0x80073D23, "The deployment operation was blocked because special profile deployment is not allowed (special profiles are user profiles where changes are discarded after the user signs out). Try logging into an account that is not a special profile. You can try logging out and logging back into the current account, or try logging into a different account.");
            yield return new ErrorCodes.ErrorCode("ERROR_DEPLOYMENT_FAILED_CONFLICTING_MUTABLE_PACKAGE_DIRECTORY", "Blocked by policy", 0x80073D24, "The deployment operation failed due to a conflicting package's mutable package directory. To install this package, remove the existing package with the conflicting mutable package directory.");
            yield return new ErrorCodes.ErrorCode("ERROR_SINGLETON_RESOURCE_INSTALLED_IN_ACTIVE_USER", "Installation failed", 0x80073D25, "The package installation failed because a singleton resource was specified and another user with that package installed is logged in. Make sure that all active users with the package installed are logged out and retry installation.");
            yield return new ErrorCodes.ErrorCode("ERROR_DIFFERENT_VERSION_OF_PACKAGED_SERVICE_INSTALLED", "Installation failed", 0x80073D26, "The package installation failed because a different version of the service is installed. Try installing a newer version of the package.");
            yield return new ErrorCodes.ErrorCode("ERROR_SERVICE_EXISTS_AS_NON_PACKAGED_SERVICE", "Installation failed", 0x80073D27, "The package installation failed because a version of the service exists outside of an .msix/.appx package. Contact your software vendor.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGED_SERVICE_REQUIRES_ADMIN_PRIVILEGES", "Admin rights required", 0x80073D28, "The package installation failed because administrator privileges are required. Contact an administrator to install this package.");
            yield return new ErrorCodes.ErrorCode("ERROR_REDIRECTION_TO_DEFAULT_ACCOUNT_NOT_ALLOWED", "Installation failed", 0x80073D29, "The package deployment failed because the operation would have redirected to default account, when the caller said not to do so.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGE_LACKS_CAPABILITY_TO_DEPLOY_ON_HOST", "Installation failed", 0x80073D2A, "The package deployment failed because the package requires a capability to natively target this host.");
            yield return new ErrorCodes.ErrorCode("ERROR_UNSIGNED_PACKAGE_INVALID_CONTENT", "Installation failed", 0x80073D2B, "The package deployment failed because its content is not valid for an unsigned package.");
            yield return new ErrorCodes.ErrorCode("ERROR_UNSIGNED_PACKAGE_INVALID_PUBLISHER_NAMESPACE", "Installation failed", 0x80073D2C, "The package deployment failed because its publisher is not in the unsigned namespace.");
            yield return new ErrorCodes.ErrorCode("ERROR_SIGNED_PACKAGE_INVALID_PUBLISHER_NAMESPACE", "Installation failed", 0x80073D2D, "The package deployment failed because its publisher is not in the signed namespace.");
            yield return new ErrorCodes.ErrorCode("ERROR_PACKAGE_EXTERNAL_LOCATION_NOT_ALLOWED", "Installation failed", 0x80073D2E, "The package deployment failed because its publisher is not in the signed namespace.");
            yield return new ErrorCodes.ErrorCode("ERROR_INSTALL_FULLTRUST_HOSTRUNTIME_REQUIRES_MAIN_PACKAGE_FULLTRUST_CAPABILITY", "Missing full trust capability", 0x80073D2F, "A host runtime dependency resolving to a package with full trust content requires the main package to have the runFullTrust capability.");
            yield return new ErrorCodes.ErrorCode("APPX_E_PACKAGING_INTERNAL", "Package API error", 0x80080200, "The packaging API has encountered an internal error.");
            yield return new ErrorCodes.ErrorCode("APPX_E_INTERLEAVING_NOT_ALLOWED", "Invalid package data", 0x80080201, "The package isn't valid because its contents are interleaved.");
            yield return new ErrorCodes.ErrorCode("APPX_E_RELATIONSHIPS_NOT_ALLOWED", "Invalid package data", 0x80080202, "The package isn't valid because it contains OPC relationships.");
            yield return new ErrorCodes.ErrorCode("APPX_E_MISSING_REQUIRED_FILE", "Invalid package data", 0x80080203, "The package isn't valid because it's missing a manifest or block map, or a code integrity file is present but a signature file is missing.\r\nEnsure that the package isn't missing one or more of these required files:\r\n\\AppxManifest.xml\r\n\\AppxBlockMap.xml\r\n\r\nIf the package contains \\AppxMetadata\\CodeIntegrity.cat, it must also contain \\AppxSignature.p7x.");
            yield return new ErrorCodes.ErrorCode("APPX_E_INVALID_MANIFEST", "Invalid package manifest", 0x80080204, "The package's AppxManifest.xml file isn't valid.");
            yield return new ErrorCodes.ErrorCode("APPX_E_INVALID_BLOCKMAP", "Invalid package data", 0x80080205, "The package's AppxBlockMap.xml file isn't valid.");
            yield return new ErrorCodes.ErrorCode("APPX_E_CORRUPT_CONTENT", "Invalid package data", 0x80080206, "The package contents can't be read because it's corrupted.");
            yield return new ErrorCodes.ErrorCode("APPX_E_BLOCK_HASH_INVALID", "Invalid package data", 0x80080207, "The computed hash value of the block doesn't match the has value stored in the block map.");
            yield return new ErrorCodes.ErrorCode("APPX_E_REQUESTED_RANGE_TOO_LARGE", "Invalid package data", 0x80080208, "The requested byte range is over 4 GB when translated to a byte range of blocks.");
            yield return new ErrorCodes.ErrorCode("TRUST_E_NOSIGNATURE", "No signature", 0x800B0100, "No signature is present in the subject.\r\nYou may get this error if the package is unsigned or the signature isn't valid. The package must be signed to be deployed.");
            yield return new ErrorCodes.ErrorCode("CERT_E_UNTRUSTEDROOT", "Root certificate not trusted", 0x800B0109, "A certificate chain processed, but terminated in a root certificate which isn't trusted by the trust provider.\r\nSee Signing a package.");
            yield return new ErrorCodes.ErrorCode("CERT_E_CHAINING", "Certificate chain error", 0x800B010A, "A certificate chain couldn't be built to a trusted root certification authority.\r\nSee Signing a package.");
            yield return new ErrorCodes.ErrorCode("APPX_E_INVALID_SIP_CLIENT_DATA", "Invalid package data", 0x80080209, "The SIP_SUBJECTINFOstructure used to sign the package didn't contain the required data");
            yield return new ErrorCodes.ErrorCode("APPX_E_INVALID_KEY_INFO", "Invalid package data", 0x8008020A, "The APPX_KEY_INFO structure used to encrypt or decrypt the package contains invalid data.");
            yield return new ErrorCodes.ErrorCode("APPX_E_INVALID_CONTENTGROUPMAP", "Invalid package data", 0x8008020B, "The .msix/.appx package's content group map is invalid.");
            yield return new ErrorCodes.ErrorCode("APPX_E_INVALID_APPINSTALLER", "Invalid app installer data", 0x8008020C, "The .appinstaller file for the package is invalid.");
            yield return new ErrorCodes.ErrorCode("APPX_E_DELTA_BASELINE_VERSION_MISMATCH", "Invalid package data", 0x8008020D, "The baseline package version in delta package does not match the version in the baseline package to be updated.");
            yield return new ErrorCodes.ErrorCode("APPX_E_DELTA_PACKAGE_MISSING_FILE", "Invalid package data", 0x8008020E, "The delta package is missing a file from the updated package.");
            yield return new ErrorCodes.ErrorCode("APPX_E_INVALID_DELTA_PACKAGE", "Invalid package data", 0x8008020F, "The delta package is invalid.");
            yield return new ErrorCodes.ErrorCode("APPX_E_DELTA_APPENDED_PACKAGE_NOT_ALLOWED", "Invalid package data", 0x80080210, "The delta appended package is not allowed for the current operation.");
            yield return new ErrorCodes.ErrorCode("APPX_E_INVALID_PACKAGING_LAYOUT", "Invalid package data", 0x80080211, "The packaging layout file is invalid.");
            yield return new ErrorCodes.ErrorCode("APPX_E_INVALID_PACKAGESIGNCONFIG", "Invalid package data", 0x80080212, "The packageSignConfig file is invalid.");
            yield return new ErrorCodes.ErrorCode("APPX_E_RESOURCESPRI_NOT_ALLOWED", "Invalid package data", 0x80080213, "The resources.pri file is not allowed when there are no resource elements in the package manifest.");
            yield return new ErrorCodes.ErrorCode("APPX_E_FILE_COMPRESSION_MISMATCH", "Invalid package data", 0x80080214, "The compression state of file in baseline and updated package does not match.");
            yield return new ErrorCodes.ErrorCode("APPX_E_INVALID_PAYLOAD_PACKAGE_EXTENSION", "Not supported extension", 0x80080215, "Non .appx extensions are not allowed for payload packages targeting older platforms.");
            yield return new ErrorCodes.ErrorCode("APPX_E_INVALID_ENCRYPTION_EXCLUSION_FILE_LIST", "Invalid package data", 0x80080216, "The encryptionExclusionFileList file is invalid.");
            // ReSharper restore StringLiteralTypo
        }
    }
}
