using System.IO;
using NUnit.Framework;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;

namespace Otor.MsixHero.Tests
{
    [TestFixture]
    public class PsfConfigReaderTests
    {
        [Test]
        public void Test()
        {
#pragma warning disable 219
            var json = @"
{
    ""applications"": [
        {
            ""id"": ""RayEval"",
            ""executable"": ""bin/RayEval.exe""
        }
    ],
    ""processes"": [
        {
            ""executable"": ""Ray.*"",
            ""fixups"": [
                {
                    ""dll"": ""TraceFixup64.dll"",
                    ""config"": {
                        ""traceMethod"": ""eventlog"",
                        ""traceLevels"": {
                            ""default"": ""always""
                        }
                    }
                },
                {
                    ""dll"": ""FileRedirectionFixup64.dll"",
                    ""config"": {                       
                        ""redirectedPaths"": {
                            ""packageRelative"": [
                                {
                                    ""base"": ""configs"",
                                    ""patterns"": [
                                        ""AutoUpdate\\.ini$""
                                    ],
                                    ""redirectTargetBase"": ""H:"",
                                    ""IsReadOnly"": ""true""
                                },
                                {
                                    ""base"": ""configs"",
                                    ""patterns"": [
                                        "".*\\.[eE][xX][eE]$"",
                                        "".*\\.[dD][lL][lL]$"",
                                        "".*\\.[tT][lL][bB]$"",
                                        "".*\\.[cC][oO][mM]$""
                                    ],
                                    ""IsExclusion"": ""true""
                                },
                                {
                                    ""base"": ""configs"",
                                    ""patterns"": [
                                        "".*""
                                    ],
                                    ""redirectTargetBase"": ""H:""
                                },
                                {
                                    ""base"": """",
                                    ""patterns"": [
                                        "".*""
                                    ]
                                }
                            ],
                        ""packageDriveRelative"": [
                            {
                                ""base"": ""temp"",
                                ""patterns"": [
                                    "".*""
                                ]
                            }
                        ],
                        ""knownFolders"": [
                            {
                                ""id"": ""ProgramFilesX64"",
                                ""relativePaths"": [
                                    {
                                        ""base"": ""Contoso\\config"",
                                        ""patterns"": [
                                            "".*""
                                        ]
                                    }
                                ]
                            }
                        ],
                        }
                    }
                }
            ]
        }
    ]
}
";
#pragma warning restore 219

            //var stringReader = new StringReader(json);

            //var configReader = new PsfReader();
            //var read = configReader.Read("RayEval", new );

            //Assert.AreEqual("bin/RayEval.exe", read.Executable);
            //Assert.Null(read.Arguments);
            //Assert.Null(read.WorkingDirectory);
            //Assert.AreEqual(PsfBitness.x64, read.Tracing);
            //Assert.AreEqual(9, read.FileRedirections.Count);
            //Assert.AreEqual(0, read.OtherFixups.Count);


        }

        [Test]
        public void ReadFromFile()
        {
            var file = Path.Combine("Resources", "ConEmuPack-O2004-M1220.603-P380-F_19.1.8.0_x64__xwfzvwzp69w2e.msix");

            using (IAppxFileReader appxFileReader = new ZipArchiveFileReaderAdapter(file))
            {
                var manifestReader = new AppxManifestReader();
                var manifest = manifestReader.Read(appxFileReader).GetAwaiter().GetResult();

                var app = manifest.Applications[0];
                var type = PackageTypeConverter.GetPackageTypeFrom(app.EntryPoint, app.Executable, app.StartPage, manifest.IsFramework);
                Assert.AreEqual(MsixPackageType.BridgePsf, type);

                Assert.AreEqual(app.Psf.Executable, "VFS\\AppVPackageDrive\\ConEmuPack\\ConEmu64.exe");
                Assert.AreEqual("VFS\\AppVPackageDrive\\ConEmuPack\\PsfLauncher1.exe", app.Executable);
            }
        }
    }
}
