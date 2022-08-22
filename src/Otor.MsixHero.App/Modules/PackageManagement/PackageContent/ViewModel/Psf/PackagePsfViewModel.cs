using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Psf.Items;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Psf.Entities;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;
using Otor.MsixHero.Appx.Psf.Entities.Interpreter;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Psf
{
    public class PackagePsfViewModel : PackageLazyLoadingViewModel
    {
        public PackagePsfViewModel(IPackageContentItemNavigation navigation)
        {
            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }
        
        public override PackageContentViewType Type => PackageContentViewType.Psf;
        
        public InterpretedPsf Psf { get; private set; }

        public ObservableCollection<ScriptViewModel> Scripts { get; private set; }

        public bool HasPsf => this.Psf != null;
        public bool HasScripts => this.Scripts?.Any() == true;

        public ICommand GoBack { get; }

        protected override async Task DoLoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            this.Psf = null;
            using var reader = FileReaderFactory.CreateFileReader(filePath);

            var paths = model.Applications.Where(a => PackageTypeConverter.GetPackageTypeFrom(a.EntryPoint, a.Executable, a.StartPage, model.IsFramework) == MsixPackageType.BridgePsf).Select(a => a.Executable).Where(a => a != null).Select(Path.GetDirectoryName).Where(a => !string.IsNullOrEmpty(a)).Distinct().ToList();
            paths.Add(string.Empty);

            var scripts = new ObservableCollection<ScriptViewModel>();
            
            foreach (var path in paths)
            {
                var configJsonPath = string.IsNullOrWhiteSpace(path) ? "config.json" : Path.Combine(path, "config.json");
                if (!reader.FileExists(configJsonPath))
                {
                    continue;
                }

                await using var s = reader.GetFile(configJsonPath);
                using var stringReader = new StreamReader(s);
                var all = await stringReader.ReadToEndAsync().ConfigureAwait(false);
                var psfSerializer = new PsfConfigSerializer();
                
                var cfg = psfSerializer.Deserialize(all);

                foreach (var app in model.Applications)
                {
                    if (app.Proxy is not PsfApplicationProxy psfProxy)
                    {
                        continue;
                    }

                    foreach (var item in psfProxy.Scripts)
                    {
                        scripts.Add(new ScriptViewModel(app.DisplayName, path, item));
                        // ReSharper disable once AssignNullToNotNullAttribute
                        var localScriptPath = Path.Combine(path, item.Name);
                        if (reader.FileExists(localScriptPath))
                        {
                            await using var fs = reader.GetFile(localScriptPath);
                            using var streamReader = new StreamReader(fs, leaveOpen: true);
                            scripts.Last().Content = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            scripts.Last().Content = null;
                        }
                    }
                }

                this.Scripts = scripts;
                this.Psf = new InterpretedPsf(cfg);
                break;
            }

            this.OnPropertyChanged(null);
        }
    }
}
