using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.Domain.Appx.Psf;

namespace otor.msixhero.lib.tests
{
    [TestFixture]
    public class PsfConfigReaderTests
    {
        [Test]
        public void Test()
        {
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
    }
}
