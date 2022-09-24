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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using MediatR;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Helpers.Tiers;
using Otor.MsixHero.App.Helpers.Update;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.App.Modules;
using Otor.MsixHero.App.Modules.Containers;
using Otor.MsixHero.App.Modules.Dialogs.About;
using Otor.MsixHero.App.Modules.Dialogs.AppAttach;
using Otor.MsixHero.App.Modules.Dialogs.AppInstaller;
using Otor.MsixHero.App.Modules.Dialogs.Dependencies;
using Otor.MsixHero.App.Modules.Dialogs.PackageExpert.ViewModels;
using Otor.MsixHero.App.Modules.Dialogs.PackageExpert.Views;
using Otor.MsixHero.App.Modules.Dialogs.Packaging;
using Otor.MsixHero.App.Modules.Dialogs.Settings;
using Otor.MsixHero.App.Modules.Dialogs.Signing;
using Otor.MsixHero.App.Modules.Dialogs.Updates;
using Otor.MsixHero.App.Modules.Dialogs.Winget;
using Otor.MsixHero.App.Modules.EventViewer;
using Otor.MsixHero.App.Modules.Main;
using Otor.MsixHero.App.Modules.Main.Shell.Views;
using Otor.MsixHero.App.Modules.PackageManagement;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent;
using Otor.MsixHero.App.Modules.SystemStatus;
using Otor.MsixHero.App.Modules.Tools;
using Otor.MsixHero.App.Modules.VolumeManagement;
using Otor.MsixHero.App.Modules.WhatsNew;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.App.Services;
using Otor.MsixHero.Appx.Diagnostic.Events;
using Otor.MsixHero.Appx.Diagnostic.Recommendations;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.ThirdParty;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Packaging.ManifestCreator;
using Otor.MsixHero.Appx.Packaging.ModificationPackages;
using Otor.MsixHero.Appx.Packaging.Packer;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.Testing;
using Otor.MsixHero.Appx.Signing.TimeStamping;
using Otor.MsixHero.Appx.Updates;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Dependencies;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Elevation.Handling;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Localization;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Infrastructure.Updates;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using Unity;
using Unity.Lifetime;

namespace Otor.MsixHero.App
{
    /// <summary>
    /// Interaction logic for the application.
    /// </summary>
    public partial class App
    {
        static App()
        {
            MsixHeroTranslation.Instance.AddResourceManager(MsixHero.App.Resources.Localization.ResourceManager, true);
            MsixHeroTranslation.Instance.AddResourceManager(Appx.Resources.Localization.ResourceManager);
            MsixHeroTranslation.Instance.AddResourceManager(Appx.Editor.Resources.Localization.ResourceManager);
            MsixHeroTranslation.Instance.AddResourceManager(AppInstaller.Resources.Localization.ResourceManager);
            MsixHeroTranslation.Instance.AddResourceManager(Dependencies.Resources.Localization.ResourceManager);
            MsixHeroTranslation.Instance.AddResourceManager(Cli.Resources.Localization.ResourceManager);
            MsixHeroTranslation.Instance.AddResourceManager(Cli.Verbs.Resources.Localization.ResourceManager);
            MsixHeroTranslation.Instance.AddResourceManager(Infrastructure.Resources.Localization.ResourceManager);
            MsixHeroTranslation.Instance.AddResourceManager(Registry.Resources.Localization.ResourceManager);
            MsixHeroTranslation.Instance.AddResourceManager(Winget.Resources.Localization.ResourceManager);

            MsixHero.App.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            Appx.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            Appx.Editor.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            AppInstaller.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            Cli.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            Cli.Verbs.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            Dependencies.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            Infrastructure.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            Registry.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;
            Winget.Resources.Localization.Culture = MsixHeroTranslation.Instance.CurrentCulture;

            MsixHeroTranslation.Instance.CultureChanged += (_, info) =>
            {
                MsixHero.App.Resources.Localization.Culture = info;
                Appx.Resources.Localization.Culture = info;
                Appx.Editor.Resources.Localization.Culture = info;
                AppInstaller.Resources.Localization.Culture = info;
                Dependencies.Resources.Localization.Culture = info;
                Cli.Verbs.Resources.Localization.Culture = info;
                Cli.Resources.Localization.Culture = info;
                Infrastructure.Resources.Localization.Culture = info;
                Registry.Resources.Localization.Culture = info;
                Winget.Resources.Localization.Culture = info;
            };

            var logLevel = MsixHeroLogLevel.Info;

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

                logLevel = config.VerboseLogging ? MsixHeroLogLevel.Trace : MsixHeroLogLevel.Info;
            });

            LogManager.Initialize(logLevel);
        }
        
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var uacClient = new SimpleUacElevationClient(new ElevatedProcessClientHandler(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "msixhero-uac.exe"), "--selfElevate --single", "--selfElevate"));
            
            // Registration of proxies for UAC handling
            uacClient.RegisterProxy<IAppxVolumeService>(this.Container);
            uacClient.RegisterProxy<IRegistryManager>(this.Container);
            uacClient.RegisterProxy<ISigningManager>(this.Container);
            uacClient.RegisterProxy<IAppxEventService>(this.Container);
            uacClient.RegisterProxy<IMsixHeroTranslationService>(this.Container);
            uacClient.RegisterProxy<IAppxPackageManagerService>(this.Container);
            uacClient.RegisterProxy<IAppxSharedPackageContainerService>(this.Container);
            uacClient.RegisterProxy<IAppxPackageQueryService>(this.Container);
            uacClient.RegisterProxy<IAppxPackageInstallationService>(this.Container);
            uacClient.RegisterProxy<IAppxPackageRunService>(this.Container);
            uacClient.RegisterProxy<IAppAttachManager>(this.Container);
            
            containerRegistry.RegisterSingleton<IUacElevation>(() => uacClient);
            containerRegistry.RegisterSingleton<IInteractionService, InteractionService>();
            containerRegistry.RegisterSingleton<IAppxVolumeService, AppxVolumeService>();

#if DEBUG
            if (NdDll.RtlGetVersion() < new Version(10, 0, 22000))
            {
                containerRegistry.RegisterSingleton<IAppxSharedPackageContainerService, AppxSharedPackageContainerWin10MockService>();
            }
            else
            {

                containerRegistry.RegisterSingleton<IAppxSharedPackageContainerService, AppxAppxSharedPackageContainerService>();
            }
#else 
            containerRegistry.RegisterSingleton<ISharedPackageContainerService, SharedPackageContainerService>();
#endif

            containerRegistry.RegisterSingleton<IRegistryManager, RegistryManager>();
            containerRegistry.RegisterSingleton<IMsixHeroTranslationService, MsixHeroTranslationService>();
            containerRegistry.RegisterSingleton<ISigningManager, SigningManager>();
            containerRegistry.RegisterSingleton<IAppxEventService, AppxEventService>();
            containerRegistry.RegisterSingleton<IAppxPackageManagerService, AppxPackageManagerService>();
            containerRegistry.RegisterSingleton<IAppxPackageQueryService, AppxPackageQueryService>();
            containerRegistry.RegisterSingleton<IAppxPackageInstallationService, AppxPackageInstallationService>();
            containerRegistry.RegisterSingleton<IAppxPackageRunService, AppxPackageRunService>();
            containerRegistry.RegisterSingleton<IAppAttachManager, AppAttachManager>();
            containerRegistry.RegisterSingleton<IAppxPacker, AppxPacker>();
            containerRegistry.RegisterSingleton<IModificationPackageBuilder, ModificationPackageBuilder>();
            containerRegistry.RegisterSingleton<IBusyManager, BusyManager>();
            containerRegistry.RegisterSingleton<IConfigurationService, LocalConfigurationService>();
            containerRegistry.RegisterSingleton<IUpdateChecker, HttpUpdateChecker>();
            containerRegistry.RegisterSingleton<IAppxFileViewer, AppxFileViewer>();
            containerRegistry.RegisterSingleton<ISigningTestService, SigningTestService>();
            containerRegistry.RegisterSingleton<ITranslationProvider, FileScanTranslationProvider>();
            containerRegistry.RegisterSingleton<IAppxUpdateImpactAnalyzer, AppxUpdateImpactAnalyzer>();
            containerRegistry.RegisterSingleton<IMsixHeroCommandExecutor, MsixHeroCommandExecutor>();
            containerRegistry.RegisterSingleton<IMsixHeroApplication, MsixHeroApplication>();
            containerRegistry.RegisterSingleton<IRunningAppsDetector, RunningAppsDetector>();
            containerRegistry.RegisterSingleton<IAppxManifestCreator, AppxManifestCreator>();
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.Register<IDependencyMapper, DependencyMapper>();
            containerRegistry.Register<IThirdPartyAppProvider, ThirdPartyAppProvider>();
            containerRegistry.Register<IServiceRecommendationAdvisor, ServiceRecommendationAdvisor>();
            containerRegistry.RegisterSingleton<PrismServices>();
            containerRegistry.RegisterSingleton<ITimeStampFeed>(_ => MsixHeroGistTimeStampFeed.CreateCached());
            containerRegistry.RegisterSingleton<IMediator>(containerProvider => new Mediator(containerProvider.Resolve));

            containerRegistry.RegisterDialog<PackageExpertDialogView, PackageExpertDialogViewModel>(NavigationPaths.DialogPaths.PackageExpert);

            if (Environment.GetCommandLineArgs().Length < 2)
            {
                // containerRegistry.RegisterDialogWindow<AcrylicDialogWindow>();
            }

            this.Container.GetContainer().RegisterMediatorHandlers(typeof(App).Assembly);
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            this.Container.Resolve<IAppxFileViewer>().Dispose();
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (this.Container.Resolve<IUacElevation>() is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule(new ModuleInfo(typeof(MainModule), ModuleNames.Main, InitializationMode.WhenAvailable));
            moduleCatalog.AddModule(new ModuleInfo(typeof(PackageManagementModule), ModuleNames.PackageManagement, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(EventViewerModule), ModuleNames.EventViewer, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(ContainersModule), ModuleNames.Containers, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(SystemStatusModule), ModuleNames.SystemStatus, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(VolumeManagementModule), ModuleNames.VolumeManagement, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(ToolsModule), ModuleNames.Tools, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(WhatsNewModule), ModuleNames.WhatsNew, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(SigningModule), ModuleNames.Dialogs.Signing, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(AppInstallerModule), ModuleNames.Dialogs.AppInstaller, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(PackagingModule), ModuleNames.Dialogs.Packaging, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(DependenciesModule), ModuleNames.Dialogs.Dependencies, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(UpdatesModule), ModuleNames.Dialogs.Updates, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(Modules.Dialogs.Volumes.VolumesModule), ModuleNames.Dialogs.Volumes, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(WingetModule), ModuleNames.Dialogs.Winget, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(AppAttachModule), ModuleNames.Dialogs.AppAttach, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(SettingsModule), ModuleNames.Dialogs.Settings, InitializationMode.OnDemand));
            moduleCatalog.AddModule(new ModuleInfo(typeof(AboutModule), ModuleNames.Dialogs.About, InitializationMode.OnDemand));

            base.ConfigureModuleCatalog(moduleCatalog);
        }

        private void InitializeMainWindow()
        {
            var regionManager = this.Container.Resolve<IRegionManager>();

            regionManager.RegisterViewWithRegion(RegionNames.Root, typeof(ShellView));
            ViewModelLocationProvider.Register<PackageExpertDialogView, PackageExpertDialogViewModel>();
            regionManager.RegisterViewWithRegion(RegionNames.PackageExpert, typeof(PackageContentHost));

            var app = this.Container.Resolve<IMsixHeroApplication>();
            var config = this.Container.Resolve<IConfigurationService>();
            var releaseNotesHelper = new ReleaseNotesHelper(config);

            if (releaseNotesHelper.ShouldShowInitialReleaseNotes())
            {
                app.CommandExecutor.Invoke(null, new SetCurrentModeCommand(ApplicationMode.WhatsNew));
            }
            else
            {
                var helper = new InitialScreen(app, config);
                helper.GoToDefaultScreenAsync();
            }
        }

        private void InitializePackageExpert()
        {
            ViewModelLocationProvider.Register<PackageExpertDialogView, PackageExpertDialogViewModel>();
            var regionManager = this.Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.PackageExpert, typeof(PackageContentHost));
            var par = new DialogParameters
            {
                {
                    "package",
                    Environment.GetCommandLineArgs()[1]
                }
            };

            var dialogService = this.Container.Resolve<IDialogService>();
            dialogService.Show(NavigationPaths.DialogPaths.PackageExpert, par, _ => { });
            // regionManager.Regions[RegionNames.Root].RequestNavigate(new Uri(NavigationPaths.PackageManagement, UriKind.Relative), par);
        }

        protected override void Initialize()
        {
            base.Initialize();

            var config = this.GetConfigurationSafe();
            var tier = config?.UiConfiguration?.UxTier ?? UxTierLevel.Auto;
            switch (tier)
            {
                case UxTierLevel.Basic:
                case UxTierLevel.Medium:
                case UxTierLevel.Rich:
                    TierController.SetCurrentTier((int)tier);
                    break;
                default:
                    TierController.SetSystemTier();
                    break;
            }

            if (Environment.GetCommandLineArgs().Length > 1)
            {
                this.InitializePackageExpert();
            }
            else
            {
                this.InitializeMainWindow();
            }
        }

        private Configuration GetConfigurationSafe()
        {
            return ExceptionGuard.Guard(() => this.Container.Resolve<IConfigurationService>().GetCurrentConfiguration());
        }

        protected override Window CreateShell()
        {
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                return null;
            }
            else
            {
                return this.Container.Resolve<MainWindow>();
            }
        }
    }

    // ReSharper disable once InconsistentNaming
    public static class IUnityContainerExtensions
    {
        public static IUnityContainer RegisterMediator(this IUnityContainer container, ITypeLifetimeManager lifetimeManager)
        {
            return container.RegisterType<IMediator, Mediator>(lifetimeManager)
                .RegisterInstance<ServiceFactory>(type =>
                {
                    var enumerableType = type
                        .GetInterfaces()
                        .Concat(new[] { type })
                        .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                    return enumerableType != null
                        ? container.ResolveAll(enumerableType.GetGenericArguments()[0])
                        : container.IsRegistered(type)
                            ? container.Resolve(type)
                            : null;
                });
        }

        public static IUnityContainer RegisterMediatorHandlers(this IUnityContainer container, Assembly assembly)
        {
            return container.RegisterTypesImplementingType(assembly, typeof(IRequestHandler<,>))
                            .RegisterNamedTypesImplementingType(assembly, typeof(INotificationHandler<>));
        }

        internal static bool IsGenericTypeOf(this Type type, Type genericType)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == genericType;
        }

        internal static void AddGenericTypes(this List<object> list, IUnityContainer container, Type genericType)
        {
            var genericHandlerRegistrations =
                container.Registrations.Where(reg => reg.RegisteredType == genericType);

            foreach (var handlerRegistration in genericHandlerRegistrations)
            {
                if (list.All(item => item.GetType() != handlerRegistration.MappedToType))
                {
                    list.Add(container.Resolve(handlerRegistration.MappedToType));
                }
            }
        }

        /// <summary>
        ///     Register all implementations of a given type for provided assembly.
        /// </summary>
        public static IUnityContainer RegisterTypesImplementingType(this IUnityContainer container, Assembly assembly, Type type)
        {
            foreach (var implementation in assembly.GetTypes().Where(t => t.GetInterfaces().Any(implementation => IsSubclassOfRawGeneric(type, implementation))))
            {
                var interfaces = implementation.GetInterfaces();
                foreach (var @interface in interfaces)
                    container.RegisterType(@interface, implementation);
            }

            return container;
        }

        /// <summary>
        ///     Register all implementations of a given type for provided assembly.
        /// </summary>
        public static IUnityContainer RegisterNamedTypesImplementingType(this IUnityContainer container, Assembly assembly, Type type)
        {
            foreach (var implementation in assembly.GetTypes().Where(t => t.GetInterfaces().Any(implementation => IsSubclassOfRawGeneric(type, implementation))))
            {
                var interfaces = implementation.GetInterfaces();
                foreach (var @interface in interfaces)
                    container.RegisterType(@interface, implementation, implementation.FullName);
            }

            return container;
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var currentType = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == currentType)
                    return true;

                toCheck = toCheck.BaseType;
            }

            return false;
        }
    }
}