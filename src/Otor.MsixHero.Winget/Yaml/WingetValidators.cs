using System.Text.RegularExpressions;

namespace Otor.MsixHero.Winget.Yaml
{
    public static class WingetValidators
    {
        public static string GetPackageIdentifierError(string packageIdentifier)
        {
            if (string.IsNullOrEmpty(packageIdentifier))
            {
                return Resources.Localization.Winget_Validation_ValueRequired;
            }

            if (packageIdentifier.Length > 128)
            {
                return string.Format(Resources.Localization.Winget_Validation_TooLongId_Format, 128);
            }

            if (!Regex.IsMatch(packageIdentifier, "^[^\\.\\s\\\\/:\\*\\?\"<>\\|\\x01-\\x1f]{1,32}(\\.[^\\.\\s\\\\/:\\*\\?\"<>\\|\\x01-\\x1f]{1,32}){1,3}$"))
            {
                return Resources.Localization.Winget_Validation_ValueInvalid;
            }

            return null;
        }

        public static string GetPackageVersionError(string packageVersion)
        {
            if (string.IsNullOrEmpty(packageVersion))
            {
                return Resources.Localization.Winget_Validation_ValueRequired;
            }

            if (packageVersion.Length > 128)
            {
                return string.Format(Resources.Localization.Winget_Validation_TooLongId_Format, 128);
            }

            if (!Regex.IsMatch(packageVersion, "^[^\\\\/:\\*\\?\"<>\\|\\x01-\\x1f]+$"))
            {
                return Resources.Localization.Winget_Validation_ValueInvalid;
            }

            return null;
        }
        
        public static string GetTagError(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return null;
            }

            if (tag.Length > 40)
            {
                return string.Format(Resources.Localization.Winget_Validation_TooLongTag_Format, 40);
            }

            return null;
        }

        public static string GetInstallerSwitchesError(string switches)
        {
            if (string.IsNullOrEmpty(switches))
            {
                return null;
            }

            if (switches.Length > 512)
            {
                return string.Format(Resources.Localization.Winget_Validation_TooLongCommand_Format, 512);
            }

            return null;
        }

        public static string GetCustomInstallerSwitchesError(string switches)
        {
            if (string.IsNullOrEmpty(switches))
            {
                return null;
            }

            if (switches.Length > 2048)
            {
                return string.Format(Resources.Localization.Winget_Validation_TooLongCommand_Format, 2048);
            }

            return null;
        }
        
        public static string GetCopyrightError(string copyright)
        {
            if (string.IsNullOrEmpty(copyright))
            {
                return null;
            }

            if (copyright.Length < 3 || copyright.Length > 512)
            {
                return string.Format(Resources.Localization.Winget_Validation_Copyright_WrongLength_Format, 3, 512);
            }

            return null;
        }

        public static string GetLicenseError(string license)
        {
            if (string.IsNullOrEmpty(license) || license.Length < 3 || license.Length > 512)
            {
                return string.Format(Resources.Localization.Winget_Validation_License_WrongLength_Format, 3, 512);
            }

            return null;
        }

        public static string GetShortDescriptionError(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                return null;
            }

            if (description.Length < 3 || description.Length > 256)
            {
                return string.Format(Resources.Localization.Winget_Validation_ShortDescription_WrongLength_Format, 3, 256);
            }

            return null;
        }

        public static string GetPackageFamilyNameError(string familyName)
        {
            if (string.IsNullOrEmpty(familyName))
            {
                return null;
            }

            if (familyName.Length > 255)
            {
                return string.Format(Resources.Localization.Winget_Validation_TooLongFamilyName_Format, 256);
            }

            if (!Regex.IsMatch(familyName, "^[A-Za-z0-9][-\\.A-Za-z0-9]+_[A-Za-z0-9]{13}$"))
            {
                return Resources.Localization.Winget_Validation_FamilyName;
            }

            return null;
        }

        public static string GetPackageNameError(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length < 2 || name.Length > 256)
            {
                return string.Format(Resources.Localization.Winget_Validation_PackageName_WrongLength_Format, 2, 256);
            }
            
            return null;
        }
        
        public static string GetPublisherError(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (name.Length < 2 || name.Length > 256)
            {
                return string.Format(Resources.Localization.Winget_Validation_Publisher_WrongLength_Format, 2, 256);
            }
            
            return null;
        }
    }
}
