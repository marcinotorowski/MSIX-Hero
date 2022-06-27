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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapplo.Log;
using Otor.MsixHero.Appx.Diagnostic.Logging;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.TimeStamping;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Localization;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.AdminHelper
{
    public class Program
    {
        private static readonly LogSource Logger = new();
        static Program()
        {
            MsixHeroTranslation.Instance.AddResourceManager(Appx.Resources.Localization.ResourceManager);
            MsixHeroTranslation.Instance.AddResourceManager(Resources.Localization.ResourceManager);
            MsixHeroTranslation.Instance.AddResourceManager(Appx.Editor.Resources.Localization.ResourceManager);
            MsixHeroTranslation.Instance.AddResourceManager(Infrastructure.Resources.Localization.ResourceManager);
            MsixHeroTranslation.Instance.AddResourceManager(Registry.Resources.Localization.ResourceManager);
            
            Appx.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            Appx.Editor.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            Infrastructure.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            Registry.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;

            MsixHeroTranslation.Instance.CultureChanged += (_, info) =>
            {
                Appx.Resources.Localization.Culture = info;
                Resources.Localization.Culture = info;
                Appx.Editor.Resources.Localization.Culture = info;
                Infrastructure.Resources.Localization.Culture = info;
                Registry.Resources.Localization.Culture = info;
            };

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            
            var logLevel = MsixHeroLogLevel.Info;

            ExceptionGuard.Guard(() =>
            {
                var service = new LocalConfigurationService();
                var config = service.GetCurrentConfiguration();
                logLevel = config.VerboseLogging ? MsixHeroLogLevel.Trace : MsixHeroLogLevel.Info;
            });

            LogManager.Initialize(logLevel);
        }

        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            var ex = e.Exception.GetBaseException();

            if (ex is OperationCanceledException)
            {
                e.SetObserved();
                return;
            }

            Logger.Warn().WriteLine(e.Exception);
        }
        
        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Logger.Error().WriteLine(ex);
            }
            else
            {
                Logger.Fatal().WriteLine(Resources.Localization.AdminHelper_Error_UnhandledException_Format, e.ExceptionObject);
            }
        }


        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0] == "--selfElevate")
                {
                    var repeat = args.All(item => item != "--single");

                    var firstLanguage = args.FirstOrDefault(a => Regex.IsMatch(a, "^[0-9]+$"));
                    if (firstLanguage != null)
                    {
                        if (!int.TryParse(firstLanguage, out var parsed))
                        {
                            Logger.Fatal().WriteLine(Resources.Localization.AdminHelper_Error_UnsupportedArgument);
                            Environment.ExitCode = 1;
                        }

                        try
                        {
                            var ci = CultureInfo.GetCultureInfo(parsed);
                            MsixHeroTranslation.Instance.ChangeCulture(ci);
                        }
                        catch (Exception e)
                        {
                            Logger.Error().WriteLine(e.Message, e);
                            Logger.Fatal().WriteLine(Resources.Localization.AdminHelper_Error_UnsupportedArgument);
                            Environment.ExitCode = 1;
                        }
                    }


                    ExceptionGuard.Guard(() =>
                    {
                        var service = new LocalConfigurationService();
                        var config = service.GetCurrentConfiguration();

                        var currentCulture = config.UiConfiguration?.Language;
                        if (!string.IsNullOrEmpty(currentCulture))
                        {
                            ExceptionGuard.Guard(() => MsixHeroTranslation.Instance.ChangeCulture(CultureInfo.GetCultureInfo(currentCulture)));

                            System.Threading.Thread.CurrentThread.CurrentCulture = MsixHeroTranslation.Instance.CurrentCulture;
                            System.Threading.Thread.CurrentThread.CurrentUICulture = MsixHeroTranslation.Instance.CurrentCulture;
                        }
                    });

                    Logger.Debug().WriteLine(Resources.Localization.AdminHelper_PipeServerStarting);

                    IConfigurationService configurationService = new LocalConfigurationService();
                    var signingManager = new SigningManager(MsixHeroGistTimeStampFeed.CreateCached());
                    var appAttachManager = new AppAttachManager(signingManager, configurationService);
                    var registryManager = new RegistryManager();
                    var appxPackageQuery = new AppxPackageQuery(registryManager, configurationService);
                    
                    var server = new SimpleElevationServer();
                    server.RegisterProxy<ISigningManager, SigningManager>(signingManager);
                    server.RegisterProxy<IAppAttachManager, AppAttachManager>(appAttachManager);
                    server.RegisterProxy<IRegistryManager, RegistryManager>(registryManager);
                    server.RegisterProxy<IAppxVolumeManager, AppxVolumeManager>();
                    server.RegisterProxy<IAppxPackageInstaller, AppxPackageInstaller>();
                    server.RegisterProxy<IAppxLogManager, AppxLogManager>();
                    server.RegisterProxy<IAppxPackageRunner, AppxPackageRunner>();
                    server.RegisterProxy<IMsixHeroTranslationService, MsixHeroTranslationService>();
                    server.RegisterProxy<IAppxPackageManager, AppxPackageManager>();
                    server.RegisterProxy<IAppxPackageQuery, AppxPackageQuery>(appxPackageQuery);
                    server.StartAsync(repeat).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                else
                {
                    Logger.Fatal().WriteLine(Resources.Localization.AdminHelper_Error_UnsupportedArgument);
                    Environment.ExitCode = 1;
                }
            }
            catch (AggregateException e)
            {
                Logger.Fatal().WriteLine(Resources.Localization.AdminHelper_Error_FatalException);
                Logger.Fatal().WriteLine(e.GetBaseException());
            }
            catch (Exception e)
            {
                Logger.Fatal().WriteLine(Resources.Localization.AdminHelper_Error_FatalException);
                Logger.Error().WriteLine(e);
            }
        }
    }
}
