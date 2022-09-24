using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;
using CommandLine.Text;
using Otor.MsixHero.Appx.Packaging.ModificationPackages;
using Otor.MsixHero.Appx.Packaging.Packer;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.TimeStamping;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Cli.Executors.Edit.Bulk;
using Otor.MsixHero.Cli.Executors.Edit.Files;
using Otor.MsixHero.Cli.Executors.Edit.Manifest;
using Otor.MsixHero.Cli.Executors.Edit.Registry;
using Otor.MsixHero.Cli.Executors.Standard;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Cli.Verbs.Edit.Bulk;
using Otor.MsixHero.Cli.Verbs.Edit.Files;
using Otor.MsixHero.Cli.Verbs.Edit.Manifest;
using Otor.MsixHero.Cli.Verbs.Edit.Registry;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.Cli.Executors
{
    internal class VerbExecutorFactory
    {
        private readonly IConsole console;

        public VerbExecutorFactory(IConsole console)
        {
            this.console = console;
        }

        public VerbExecutor CreateStandardVerbExecutor(IEnumerable<string> arguments)
        {
            VerbExecutor verbExecutor = null;

            var p = Parser.Default.ParseArguments(
                arguments,
                typeof(SignVerb),
                typeof(PackVerb),
                typeof(UnpackVerb),
                typeof(NewCertVerb),
                typeof(TrustVerb),
                typeof(SharedPackageContainerVerb),
                typeof(AppAttachVerb),
                typeof(NewModPackVerb),
                typeof(ExtractCertVerb),
                typeof(DependenciesVerb),
                typeof(UpdateImpactVerb),
                typeof(EditVerbPlaceholder));
            p.WithParsed<SignVerb>(verb =>
            {
                verbExecutor = new SignVerbExecutor(verb, new SigningManager(MsixHeroGistTimeStampFeed.CreateCached()), new LocalConfigurationService(), this.console);
            });

            p.WithParsed<PackVerb>(verb =>
            {
                verbExecutor = new PackVerbExecutor(verb, this.console);
            });


#if DEBUG
            if (NdDll.RtlGetVersion() < new Version(10, 0, 22000))
            {
                p.WithParsed<SharedPackageContainerVerb>(verb =>
                {
                    verbExecutor = new SharedPackageContainerVerbExecutor(verb, new AppxSharedPackageContainerWin10MockService(), this.console);
                });
            }
            else
            {
                p.WithParsed<SharedPackageContainerVerb>(verb =>
                {
                    verbExecutor = new SharedPackageContainerVerbExecutor(verb, new AppxAppxSharedPackageContainerService(), this.console);
                });
            }
#else
            p.WithParsed<SharedPackageContainerVerb>(verb =>
            {
                verbExecutor = new SharedPackageContainerVerbExecutor(verb, new SharedPackageContainerService(), this.console);
            });
#endif

            p.WithParsed<UnpackVerb>(verb =>
            {
                verbExecutor = new UnpackVerbExecutor(verb, this.console);
            });

            p.WithParsed<NewCertVerb>(verb =>
            {
                verbExecutor = new NewCertVerbExecutor(verb, new SigningManager(MsixHeroGistTimeStampFeed.CreateCached()), this.console);
            });

            p.WithParsed<NewModPackVerb>(verb =>
            {
                verbExecutor = new NewModPackVerbExecutor(verb, new ModificationPackageBuilder(new AppxPacker()), this.console);
            });

            p.WithParsed<TrustVerb>(verb =>
            {
                verbExecutor = new TrustVerbExecutor(verb, new SigningManager(MsixHeroGistTimeStampFeed.CreateCached()), this.console);
            });

            p.WithParsed<ExtractCertVerb>(verb =>
            {
                verbExecutor = new ExtractCertVerbExecutor(verb, new SigningManager(MsixHeroGistTimeStampFeed.CreateCached()), this.console);
            });

            p.WithParsed<AppAttachVerb>(verb =>
            {
                verbExecutor = new AppAttachVerbExecutor(verb, new AppAttachManager(new SigningManager(MsixHeroGistTimeStampFeed.CreateCached()), new LocalConfigurationService()), this.console);
            });

            p.WithParsed<UpdateImpactVerb>(verb =>
            {
                verbExecutor = new UpdateImpactVerbExecutor(verb, this.console);
            });

            p.WithParsed<DependenciesVerb>(verb =>
            {
                verbExecutor = new DependenciesVerbExecutor(verb, this.console);
            });

            p.WithParsed<EditVerbPlaceholder>(_ => Task.FromResult(0));

            p.WithNotParsed(arg =>
            {
                var err = arg.FirstOrDefault();
                if (err != null)
                {
                    Environment.ExitCode = (int)err.Tag;
                }
                else
                {
                    Environment.ExitCode = StandardExitCodes.ErrorGeneric;
                }
            });

            return verbExecutor;
        }

        public void ThrowIfInvalidArguments(IEnumerable<string> arguments, bool inBulkMode)
        {
            GetEditParser(arguments, inBulkMode);
        }

        public VerbExecutor CreateEditVerbExecutor(string packagePath, IEnumerable<string> arguments, bool inBulkMode = false)
        {
            var p = GetEditParser(arguments, inBulkMode);
            
            VerbExecutor verbExecutor = null;
            p.WithParsed<DeleteFileEditVerb>(verb =>
            {
                verbExecutor = new DeleteFileEditVerbExecutor(packagePath, verb, this.console);
            });

            p.WithParsed<AddCapabilityVerb>(verb =>
            {
                verbExecutor = new AddCapabilityVerbExecutor(packagePath, verb, this.console);
            });

            p.WithParsed<SetIdentityEditVerb>(verb =>
            {
                verbExecutor = new SetIdentityVerbExecutor(packagePath, verb, this.console);
            });

            p.WithParsed<SetPropertiesEditVerb>(verb =>
            {
                verbExecutor = new SetPropertiesVerbExecutor(packagePath, verb, this.console);
            });

            p.WithParsed<SetBuildMetaDataVerb>(verb =>
            {
                verbExecutor = new SetBuildMetaDataVerbExecutor(packagePath, verb, this.console);
            });

            p.WithParsed<AddFileEditVerb>(verb =>
            {
                verbExecutor = new AddFileEditVerbExecutor(packagePath, verb, this.console);
            });

            p.WithParsed<ImportRegistryEditVerb>(verb =>
            {
                verbExecutor = new ImportRegistryVerbExecutor(packagePath, verb, this.console);
            });

            p.WithParsed<SetRegistryKeyEditVerb>(verb =>
            {
                verbExecutor = new SetRegistryKeyVerbExecutor(packagePath, verb, this.console);
            });

            p.WithParsed<SetRegistryValueEditVerb>(verb =>
            {
                verbExecutor = new SetRegistryValueVerbExecutor(packagePath, verb, this.console);
            });

            p.WithParsed<DeleteRegistryKeyEditVerb>(verb =>
            {
                verbExecutor = new DeleteRegistryKeyVerbExecutor(packagePath, verb, this.console);
            });

            p.WithParsed<DeleteRegistryValueEditVerb>(verb =>
            {
                verbExecutor = new DeleteRegistryValueVerbExecutor(packagePath, verb, this.console);
            });

            if (!inBulkMode)
            {
                p.WithParsed<ListEditVerb>(verb =>
                {
                    verbExecutor = new ListEditVerbExecutor(packagePath, verb, this.console);
                });
            }
            
            return verbExecutor;
        }

        private static ParserResult<object> GetEditParser(IEnumerable<string> arguments, bool inBulkMode)
        {
            ParserResult<object> p;
            Parser parser;

            var types = new List<Type>
            {
                typeof(SetIdentityEditVerb),
                typeof(SetPropertiesEditVerb),
                typeof(AddCapabilityVerb),
                typeof(SetBuildMetaDataVerb),
                typeof(DeleteFileEditVerb),
                typeof(AddFileEditVerb),
                typeof(SetRegistryKeyEditVerb),
                typeof(SetRegistryValueEditVerb),
                typeof(DeleteRegistryKeyEditVerb),
                typeof(DeleteRegistryValueEditVerb),
                typeof(ImportRegistryEditVerb)
            };

            if (!inBulkMode)
            {
                types.Add(typeof(ListEditVerb));
            }

            if (!inBulkMode)
            {
                parser = Parser.Default;
            }
            else
            {
                parser = new Parser(a => a.AutoHelp = false);
            }

            var parsed = parser.ParseArguments(arguments, types.ToArray());

            p = parsed.WithNotParsed(arg =>
            {
                // ReSharper disable once PossibleMultipleEnumeration
                var err = arg.FirstOrDefault();
                if (err != null)
                {
                    Environment.ExitCode = (int)err.Tag;
                }
                else
                {
                    Environment.ExitCode = StandardExitCodes.ErrorGeneric;
                }

                if (inBulkMode)
                {
                    var formatter = SentenceBuilder.Factory().FormatError;
                    // ReSharper disable once PossibleMultipleEnumeration
                    throw new ArgumentsParsingException(arg.Select(ne => formatter(ne)).ToArray());
                }
            });

            return p;
        }

        public class ArgumentsParsingException : Exception
        {
            public ArgumentsParsingException(params string[] errors)
            {
                this.Errors = errors;
            }
            
            public string[] Errors { get; }
        }
    }
}
