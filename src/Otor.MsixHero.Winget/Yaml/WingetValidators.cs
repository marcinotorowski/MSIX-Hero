using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Otor.MsixHero.Winget.Yaml.Entities;

namespace Otor.MsixHero.Winget.Yaml
{
    public static class WingetValidators
    {
        public static string GetPackageIdentifierError(string packageIdentifier)
        {
            if (string.IsNullOrEmpty(packageIdentifier))
            {
                return "This value is required.";
            }

            if (packageIdentifier.Length > 128)
            {
                return "The identifier may not be longer than 128 characters.";
            }

            if (!Regex.IsMatch(packageIdentifier, "^[^\\.\\s\\\\/:\\*\\?\"<>\\|\\x01-\\x1f]{1,32}(\\.[^\\.\\s\\\\/:\\*\\?\"<>\\|\\x01-\\x1f]{1,32}){1,3}$"))
            {
                return "The value is invalid according to Winget requirements.";
            }

            return null;
        }

        public static string GetPackageVersionError(string packageVersion)
        {
            if (string.IsNullOrEmpty(packageVersion))
            {
                return "This value is required.";
            }

            if (packageVersion.Length > 128)
            {
                return "The identifier may not be longer than 128 characters.";
            }

            if (!Regex.IsMatch(packageVersion, "^[^\\\\/:\\*\\?\"<>\\|\\x01-\\x1f]+$"))
            {
                return "The value is invalid according to Winget requirements.";
            }

            return null;
        }

        public static string GetLocaleError(string locale)
        {
            if (string.IsNullOrEmpty(locale))
            {
                return null;
            }

            if (!Regex.IsMatch(locale, "^([a-zA-Z]{2}|[iI]-[a-zA-Z]+|[xX]-[a-zA-Z]{1,8})(-[a-zA-Z]{1,8})*$"))
            {
                return "The value is invalid according to Winget requirements.";
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
                return "The tag may not be longer than 40 characters.";
            }

            return null;
        }

        public static string GetChannelError(string channel)
        {
            if (string.IsNullOrEmpty(channel))
            {
                return null;
            }

            if (channel.Length > 16)
            {
                return "The channel may not be longer than 16 characters.";
            }

            return null;
        }

        public static string GetPlatformsError(IReadOnlyCollection<YamlPlatform> platforms)
        {
            if (platforms == null)
            {
                return null;
            }

            if (platforms.Count > 2)
            {
                return "Only two platforms are allowed.";
            }

            if (platforms.Count == 2 && platforms.First() == platforms.Last())
            {
                return "The platforms must be unique.";
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
                return "The length of the command may not exceed 512.";
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
                return "The length of the command may not exceed 2048.";
            }

            return null;
        }

        public static string GetInstallerSuccessCodeError(IReadOnlyCollection<int> errorCodes)
        {
            if (errorCodes == null || errorCodes.Any())
            {
                return null;
            }

            if (errorCodes.Count > 16)
            {
                return "Only up to 16 error codes can be defined.";
            }

            if (errorCodes.Contains(0))
            {
                return "Exit code 0 is not accepted.";
            }

            var duplicated = string.Join(", ", errorCodes.GroupBy(c => c).Select(c => new { Code = c.Key, Count = c.Count() }).Where(c => c.Count > 1));
            if (!string.IsNullOrEmpty(duplicated))
            {
                return "The following values are duplicated: " + duplicated;
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
                return "The package copyright must be between 3 and 512 characters.";
            }

            return null;
        }

        public static string GetLicenseError(string license)
        {
            if (string.IsNullOrEmpty(license) || license.Length < 3 || license.Length > 512)
            {
                return "The package license must be between 3 and 512 characters.";
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
                return "The short package description must be between 3 and 256 characters.";
            }

            return null;
        }

        public static string GetDescriptionError(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                return null;
            }

            if (description.Length < 3 || description.Length > 1000)
            {
                return "The full package description must be between 3 and 1000 characters.";
            }

            return null;
        }

        public static string GetTagsError(string tags)
        {
            if (string.IsNullOrEmpty(tags))
            {
                return null;
            }
            var tagsList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (tagsList.Length == 0)
            {
                return null;
            }

            if (tagsList.Length > 16)
            {
                return "Up to 16 tags are supported.";
            }

            var duplicated = string.Join(", ", tagsList.GroupBy(c => c).Select(c => new { Code = c.Key, Count = c.Count() }).Where(c => c.Count > 1));
            if (!string.IsNullOrEmpty(duplicated))
            {
                return "The following values are duplicated: " + duplicated;
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
                return "Package family name may not be longer than 255 characters.";
            }

            if (!Regex.IsMatch(familyName, "^[A-Za-z0-9][-\\.A-Za-z0-9]+_[A-Za-z0-9]{13}$"))
            {
                return "Package family name is invalid according to Winget requirements.";
            }

            return null;
        }

        public static string GetPackageNameError(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length < 2 || name.Length > 256)
            {
                return "Package name must have between 2 and 256 characters.";
            }
            
            return null;
        }

        public static string GetAuthorError(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (name.Length < 2 || name.Length > 256)
            {
                return "Author name must have between 2 and 256 characters.";
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
                return "Publisher name must have between 2 and 256 characters.";
            }
            
            return null;
        }
    }
}
