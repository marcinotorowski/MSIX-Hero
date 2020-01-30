using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using otor.msixhero.lib.Domain.Appx.Psf;

namespace otor.msixhero.lib.tests
{
    [TestFixture]
    public class PsfTests
    {
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
