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
using System.Collections.Generic;
using System.IO;

namespace Otor.MsixHero.Registry.Tokenizer
{
    public class RegistryTokenizer
    {
        private readonly IDictionary<string, string> _tokenPaths = new Dictionary<string, string>();

        public string Tokenize(string path)
        {
            if (this._tokenPaths.Count == 0)
            {
                this.Initialize();
            }

            var shouldHaveBackslash = path.EndsWith('\\');
            if (shouldHaveBackslash)
            {
                path = path.TrimEnd('\\');
            }

            if (this._tokenPaths.TryGetValue(path, out var matched))
            {
                if (shouldHaveBackslash)
                {
                    return $"[{{{matched}}}]\\";
                }

                return $"[{{{matched}}}]";
            }

            string previousTokenMatch = null;
            var restIndex = 0;
            var indexOfBackSlash = 0;

            while ((indexOfBackSlash = path.IndexOf('\\', indexOfBackSlash + 1)) != -1)
            {
                var pathToMatch = path.Substring(0, indexOfBackSlash);
                if (!this._tokenPaths.TryGetValue(pathToMatch, out matched))
                {
                    if (restIndex == 0)
                    {
                        continue;
                    }

                    break;
                }

                restIndex = indexOfBackSlash + 1;
                previousTokenMatch = matched;
            }

            if (previousTokenMatch == null)
            {
                return path;
            }

            var newPath = ("[{" + previousTokenMatch + "}]\\" + path.Substring(restIndex)).TrimEnd('\\');

            if (shouldHaveBackslash)
            {
                return newPath + '\\';
            }

            return newPath;
        }

        private void AddToken(string token, string fullPath)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (fullPath == null)
            {
                throw new ArgumentNullException(nameof(fullPath));
            }

            if (fullPath.StartsWith('%'))
            {
                fullPath = Environment.ExpandEnvironmentVariables(fullPath);
                if (fullPath.StartsWith('%'))
                {
                    // A workaround for endless loops. Do not proceed if resolved path still starts with %.
                    return;
                }
            }

            this._tokenPaths[fullPath.TrimEnd('\\')] = token;
        }

        private void AddToken(string token, Environment.SpecialFolder fullPath)
        {
            this.AddToken(token, Environment.GetFolderPath(fullPath));
        }

        private void AddToken(string token, Environment.SpecialFolder fullPath, string relativePath)
        {
            this.AddToken(token, Path.Combine(Environment.GetFolderPath(fullPath), relativePath));
        }

        private void Initialize()
        {
            this.AddToken("AccountPictures",  Environment.SpecialFolder.ApplicationData, @"Microsoft\Windows\AccountPictures");
            this.AddToken("Administrative Tools", Environment.SpecialFolder.AdminTools);
            this.AddToken("AppData", Environment.SpecialFolder.ApplicationData);
            this.AddToken("Application Shortcuts", Environment.SpecialFolder.LocalApplicationData, @"Microsoft\Windows\Application Shortcuts");
            this.AddToken("Cache", Environment.SpecialFolder.InternetCache);
            this.AddToken("CD Burning", Environment.SpecialFolder.CDBurning);
            this.AddToken("Common Administrative Tools", Environment.SpecialFolder.CommonAdminTools);
            this.AddToken("Common AppData", Environment.SpecialFolder.CommonApplicationData);
            this.AddToken("Common Desktop", Environment.SpecialFolder.CommonDesktopDirectory);
            this.AddToken("Common Documents", Environment.SpecialFolder.CommonDocuments);
            this.AddToken("Common Programs", Environment.SpecialFolder.CommonPrograms);
            this.AddToken("Common Start", Environment.SpecialFolder.CommonStartMenu);
            this.AddToken("Common Startup", Environment.SpecialFolder.CommonStartup);
            this.AddToken("Common Templates", Environment.SpecialFolder.CommonTemplates);
            this.AddToken("CommonDownloads", @"%PUBLIC%\Downloads");
            this.AddToken("CommonMusic", Environment.SpecialFolder.CommonMusic);
            this.AddToken("CommonPictures", Environment.SpecialFolder.CommonPictures);
            this.AddToken("CommonRingtones", @"%PROGRAMDATA%\Microsoft\Windows\Ringtones");
            this.AddToken("CommonVideo", Environment.SpecialFolder.CommonVideos);
            this.AddToken("Contacts", @"%HOMEDRIVE%%HOMEPATH%\Contacts");
            this.AddToken("CredentialManager",  Environment.SpecialFolder.ApplicationData,@"Microsoft\Credentials");
            this.AddToken("Cache", Environment.SpecialFolder.ApplicationData, @"Microsoft\Crypto");
            this.AddToken("Desktop", Environment.SpecialFolder.Desktop);
            this.AddToken("Device Metadata Store", Environment.SpecialFolder.CommonApplicationData, @"Microsoft\Windows\DeviceMetadataStore");
            this.AddToken("DocumentsLibrary", Environment.SpecialFolder.ApplicationData, @"Microsoft\Windows\Libraries\Documents.library-ms");
            this.AddToken("Downloads", @"%HOMEDRIVE%%HOMEPATH%\Downloads");
            this.AddToken("DpapiKeys", Environment.SpecialFolder.ApplicationData, @"Microsoft\Protect");
            this.AddToken("Favorites", Environment.SpecialFolder.Favorites);
            this.AddToken("Fonts", Environment.SpecialFolder.Fonts);
            this.AddToken("GameTasks", Environment.SpecialFolder.LocalApplicationData, @"Microsoft\Windows\GameExplorer");
            this.AddToken("History", Environment.SpecialFolder.History);
            this.AddToken("ImplicitAppShortcuts", Environment.SpecialFolder.ApplicationData, @"Microsoft\Internet Explorer\Quick Launch\User Pinned\ImplicitAppShortcuts");
            this.AddToken("Libraries", Environment.SpecialFolder.ApplicationData, @"Microsoft\Windows\Libraries");
            this.AddToken("Links", @"%HOMEDRIVE%%HOMEPATH%\Links");
            this.AddToken("Local AppData", Environment.SpecialFolder.LocalApplicationData);
            this.AddToken("LocalAppDataLow", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).TrimEnd('\\') + "Low");
            this.AddToken("MusicLibrary", Environment.SpecialFolder.ApplicationData, @"Microsoft\Windows\Libraries\Music.library-ms");
            this.AddToken("My Music", Environment.SpecialFolder.MyMusic);
            this.AddToken("My Pictures", Environment.SpecialFolder.MyPictures);
            this.AddToken("My Video", Environment.SpecialFolder.MyVideos);
            this.AddToken("NetHood", Environment.SpecialFolder.NetworkShortcuts);
            this.AddToken("Personal", Environment.SpecialFolder.Personal);
            this.AddToken("PicturesLibrary", Environment.SpecialFolder.ApplicationData, @"Microsoft\Windows\Libraries\Pictures.library-ms");
            this.AddToken("Podcast Library", Environment.SpecialFolder.ApplicationData, @"Microsoft\Windows\Libraries\Podcasts.library-ms");
            this.AddToken("Podcasts", @"%HOMEDRIVE%%HOMEPATH%\Podcasts");
            this.AddToken("PrintHood", Environment.SpecialFolder.ApplicationData, @"Microsoft\Windows\Printer");
            this.AddToken("Profile", @"%HOMEDRIVE%%HOMEPATH%");
            this.AddToken("ProgramFiles", Environment.SpecialFolder.ProgramFiles);
            this.AddToken("ProgramFilesCommon", Environment.SpecialFolder.CommonProgramFiles);
            this.AddToken("ProgramFilesCommonX64", Environment.SpecialFolder.CommonProgramFiles);
            this.AddToken("ProgramFilesCommonX86", Environment.SpecialFolder.CommonProgramFilesX86);
            this.AddToken("ProgramFilesX64", Environment.SpecialFolder.ProgramFiles);
            this.AddToken("ProgramFilesX86", Environment.SpecialFolder.ProgramFilesX86);
            this.AddToken("Programs", Environment.SpecialFolder.Programs);
            this.AddToken("Public", @"%PUBLIC%");
            this.AddToken("PublicAccountPictures", @"%PUBLIC%\AccountPictures");
            this.AddToken("PublicGameTasks", Environment.SpecialFolder.CommonApplicationData, @"Microsoft\Windows\GameExplorer");
            this.AddToken("PublicLibraries", @"%PUBLIC%\Libraries");
            this.AddToken("Quick Launch ", Environment.SpecialFolder.ApplicationData, @"Microsoft\Internet Explorer\Quick Launch");
            this.AddToken("Recent", Environment.SpecialFolder.Recent);
            this.AddToken("RecordedTVLibrary", @"%PUBLIC%\Libraries\RecordedTV.library-ms");
            this.AddToken("ResourceDir", Environment.SpecialFolder.Windows, "resources");
            this.AddToken("Ringtones", Environment.SpecialFolder.LocalApplicationData, @"Microsoft\Windows\Ringtones");
            this.AddToken("Roamed Tile Images", Environment.SpecialFolder.LocalApplicationData, @"Microsoft\Windows\RoamedTileImages");
            this.AddToken("Roaming Tiles", Environment.SpecialFolder.LocalApplicationData, @"Microsoft\Windows\RoamingTiles");
            this.AddToken("SavedGames", @"%HOMEDRIVE%%HOMEPATH%\Saved Games");
            this.AddToken("Searches", @"%HOMEDRIVE%%HOMEPATH%\Searches");
            this.AddToken("SendTo", Environment.SpecialFolder.SendTo);
            this.AddToken("Start Menu", Environment.SpecialFolder.StartMenu);
            this.AddToken("Startup", Environment.SpecialFolder.Startup);
            this.AddToken("System", Environment.SpecialFolder.System);
            this.AddToken("SystemCertificates", Environment.SpecialFolder.ApplicationData, @"Microsoft\SystemCertificates");
            this.AddToken("SystemX86", Environment.SpecialFolder.SystemX86);
            this.AddToken("Templates", Environment.SpecialFolder.Templates);
            this.AddToken("User Pinned", Environment.SpecialFolder.ApplicationData, @"Microsoft\Internet Explorer\Quick Launch\User Pinned");
            this.AddToken("UserProfiles", Path.GetDirectoryName(@"%HOMEDRIVE%%HOMEPATH%"));
            this.AddToken("VideosLibrary", Environment.SpecialFolder.ApplicationData, @"Microsoft\Windows\Libraries\Videos.library-ms");
            this.AddToken("Windows", Environment.SpecialFolder.Windows);
        }
    }
}
