using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IniParser.Model.Configuration;
using IniParser.Parser;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;

namespace Otor.MsixHero.Appx.Psf;

public class MsixHelperProxyReader(string applicationId, string originalEntryPoint, IAppxFileReader fileReader) : ApplicationProxyReader
{
    private readonly string _applicationId = applicationId;
    private readonly string _originalEntryPoint = originalEntryPoint;
    private readonly IAppxFileReader _fileReader = fileReader;

    public override async Task<BaseApplicationProxy> Inspect(CancellationToken cancellationToken = default)
    {
        var iniDirectoryPath = Path.GetDirectoryName(this._originalEntryPoint);
        var iniFileName = Path.ChangeExtension(this._originalEntryPoint, ".ini");

        if (iniFileName == null)
        {
            return null;
        }

        if (!this._fileReader.FileExists(iniFileName))
        {
            return new MsixHelperApplicationProxy();
        }

        var iniFullPath = iniDirectoryPath == null ? iniFileName : Path.Combine(iniDirectoryPath, iniFileName);
        await using var iniFileStream = this._fileReader.GetFile(iniFullPath);
        var idp = new IniDataParser(new IniParserConfiguration()
        {
            ThrowExceptionsOnError = true,
            SkipInvalidLines = true
        });

        var iniParser = new IniParser.StreamIniDataParser(idp);

        using var iniFileStreamReader = new StreamReader(iniFileStream);
        var iniData = iniParser.ReadData(iniFileStreamReader);


        /*
         SAMPLE:
           [ApplicationOne]
           Target=%ProgramFiles(x86)%\MyApp\MyApplication1.exe
           Options=
           WorkingDir=%ProgramFiles(x86)%\MyApp
           SetEnv=Environment
           PreLaunch=PreLaunchScript
           Param1=/test
           Param2=/secondParam
           Param3=/noError

           [PreLaunchScript]
           FolderCreate, %LOCALAPPDATA%\MyApp
           StringReplace,%WorkingDir%\conf.cfg,<AppData>,%APPDATA%,%LOCALAPPDATA%\MyApp\conf.cfg,1
           RegWrite, REG_SZ, HKEY_CURRENT_USER\Software\MyApp, WorkingDir, %WorkingDir%*/

        var findSection = iniData.Sections[this._applicationId];
        if (findSection == null)
        {
            return new MsixHelperApplicationProxy();
        }

        var target = findSection["Target"];
        var workingDirectory = findSection["WorkingDir"];

        var arguments = new StringBuilder();
        var currentIndex = 1;
        while (true)
        {
            var param = findSection["Param" + currentIndex];
            if (string.IsNullOrEmpty(param))
            {
                break;
            }

            arguments.Append(" ");
            arguments.Append(param);
            currentIndex++;
        }

        var app = new MsixHelperApplicationProxy
        {
            Arguments = arguments.Length > 0 ? arguments.ToString(1, arguments.Length - 1) : null,
            Executable = target,
            WorkingDirectory = workingDirectory
        };

        return app;
    }
}