using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;
using Dapplo.Log;

namespace Otor.MsixHero.Infrastructure.Localization
{
    public class MsixHeroTranslation : INotifyPropertyChanged
    {
        private static readonly LogSource Logger = new();

        private readonly Dictionary<string, ResourceManager> _resourceManagerDictionary = new();

        private CultureInfo _currentCulture = CultureInfo.InstalledUICulture;

        private string _defaultDictionary;

        public static MsixHeroTranslation Instance { get; } = new();

        /// <summary>
        /// Gets a translated version of a string with a given <see cref="key"/>.
        /// </summary>
        /// <param name="key">The identifier of the resource to translate.</param>
        /// <returns>Translated version of a string.</returns>
        /// <remarks>Pass a name in format X.Y where X is the name of the assembly containing the translation,
        /// and Y is the unique key of the translation in that assembly. If you do not provide the assembly,
        /// the default resource dictionary will be used.</remarks>
        public string this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (key.Length == 0)
                {
                    throw new ArgumentException(@"The key must not be empty.", nameof(key));
                }

                string baseName;
                string lookupName;

                if (key[0] == '.')
                {
                    baseName = this._defaultDictionary;
                    lookupName = key[1..];
                }
                else
                {
                    var lastDotIndex = key.LastIndexOf('.');
                    
                    if (lastDotIndex <= 0 && this._defaultDictionary != null)
                    {
                        baseName = this._defaultDictionary;
                        lookupName = key;
                    }
                    else
                    {
                        baseName = key.Substring(0, lastDotIndex);
                        lookupName = key.Remove(0, lastDotIndex + 1);
                    }
                }

                if (!string.IsNullOrEmpty(baseName) && _resourceManagerDictionary.TryGetValue(baseName, out var dictionary))
                {
                    return dictionary.GetString(lookupName, _currentCulture) ?? "<" + key + ">";
                }

                return "<" + key + ">";
            }
        }

        public void ChangeCulture(CultureInfo cultureInfo)
        {
            this.CurrentCulture = cultureInfo;
        }

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            private set
            {
                if (value == null)
                {
                    value = CultureInfo.InstalledUICulture;
                }

                if (this._currentCulture == value)
                {
                    return;
                }

                Logger.Info().WriteLine("Changing current culture from '{0}' to '{1}'…", this.CurrentCulture.Name, value.Name);

                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
                this._currentCulture = value;

                this.CultureChanged?.Invoke(this, this._currentCulture);
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            }
        }

        public void AddResourceManager(ResourceManager resourceManager, bool asDefault = false)
        {
            _resourceManagerDictionary.TryAdd(resourceManager.BaseName, resourceManager);

            if (asDefault)
            {
                this._defaultDictionary = resourceManager.BaseName;
            }
        }

        public event EventHandler<CultureInfo> CultureChanged; 

        public event PropertyChangedEventHandler PropertyChanged;
    }
}