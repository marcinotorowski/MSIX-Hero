// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

using System.IO;
using NUnit.Framework;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Packaging.ManifestCreator;
using Otor.MsixHero.Appx.Psf.Entities;

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
                Assert.That(type, Is.EqualTo(MsixPackageType.Win32Psf));

                Assert.That("VFS\\AppVPackageDrive\\ConEmuPack\\ConEmu64.exe", Is.EqualTo(app.Proxy.Executable));
                Assert.That(app.Executable, Is.EqualTo("VFS\\AppVPackageDrive\\ConEmuPack\\PsfLauncher1.exe"));
            }
        }

        [Test]
        public void TestScripts()
        {
            var configJson = @"{
  ""applications"": [
    {
      ""id"": ""Sample"",
      ""executable"": ""Sample.exe"",
      ""workingDirectory"": """",
      ""stopOnScriptError"": false,
      ""startScript"":
      {
        ""scriptPath"": ""RunMePlease.ps1"",
        ""scriptArguments"": ""\""First argument\"" secondArgument"",
        ""runInVirtualEnvironment"": true,
        ""showWindow"": true,
        ""waitForScriptToFinish"": false
      },
      ""endScript"":
      {
        ""scriptPath"": ""RunMeAfter.ps1"",
        ""scriptArguments"": ""ThisIsMe.txt""
      }
    },
    {
      ""id"": ""CPPSample"",
      ""executable"": ""CPPSample.exe"",
      ""workingDirectory"": """",
      ""startScript"":
      {
        ""scriptPath"": ""CPPStart.ps1"",
        ""scriptArguments"": ""ThisIsMe.txt"",
        ""runInVirtualEnvironment"": true
      },
      ""endScript"":
      {
        ""scriptPath"": ""CPPEnd.ps1"",
        ""scriptArguments"": ""ThisIsMe.txt"",
        ""runOnce"": false
      }
    }
  ],
  ""processes"": [
  ]
}";
            var obj = new PsfConfigSerializer().Deserialize(configJson);

            Assert.That(obj.Applications.Count, Is.EqualTo(2));

            var sample = obj.Applications[0];
            Assert.That(sample.StartScript, Is.Not.Null);
            Assert.That(sample.StartScript.ScriptPath, Is.EqualTo("RunMePlease.ps1"));
            Assert.That(sample.StartScript.ScriptArguments, Is.EqualTo("\"First argument\" secondArgument"));
            Assert.That(sample.StartScript.RunInVirtualEnvironment, Is.True);
            Assert.That(sample.StartScript.ShowWindow, Is.True);
            Assert.That(sample.StartScript.WaitForScriptToFinish, Is.False);
            Assert.That(sample.EndScript, Is.Not.Null);
            Assert.That(sample.EndScript.ScriptPath, Is.EqualTo("RunMeAfter.ps1"));
            Assert.That(sample.EndScript.ScriptArguments, Is.EqualTo("ThisIsMe.txt"));

            var cppSample = obj.Applications[1];
        }

        [Test]
        public void TestDeserialization()
        {
            var jsonString = @"{""applications"": [
                {
                    ""id"": ""App"",
                    ""executable"": ""ClearbyteMSIXSample/ClearbyteMSIXSample.exe"",
                    ""workingDirectory"": ""ClearbyteMSIXSample/""
                }
                ],
                ""processes"": [
                {
                    ""executable"": ""ClearbyteMSIXSample"",
                    ""fixups"": [
                    {
                        ""dll"": ""FileRedirectionFixup32.dll"",
                        ""config"": {
    ""redirectedPaths"": {
        ""packageRelative"": [
            {
                ""base"": ""logs"",
                ""patterns"": [
                    "".*\\.log""
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
            },
            {
                ""id"": "" {FDD39AD0-238F-46AF-ADB4-6C85480369C7}"",
                ""relativePaths"": [
                    {
                        ""base"": ""MyApplication"",
                        ""patterns"": [
                            "".*""
                        ]
                    }
                ]
            }
        ]
    }
}
                    }
                    ]
                }
                ]
            }";

            var obj = new PsfConfigSerializer().Deserialize(jsonString);
        }
    }
}
