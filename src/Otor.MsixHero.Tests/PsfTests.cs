using NUnit.Framework;
using Otor.MsixHero.Appx.Psf.Entities;

namespace Otor.MsixHero.Tests
{
    [TestFixture]
    public class PsfTests
    {
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

            Assert.AreEqual(2, obj.Applications.Count);

            var sample = obj.Applications[0];
            Assert.NotNull(sample.StartScript);
            Assert.AreEqual("RunMePlease.ps1", sample.StartScript.ScriptPath);
            Assert.AreEqual("\"First argument\" secondArgument", sample.StartScript.ScriptArguments);
            Assert.IsTrue(sample.StartScript.RunInVirtualEnvironment);
            Assert.IsTrue(sample.StartScript.ShowWindow);
            Assert.IsFalse(sample.StartScript.WaitForScriptToFinish);
            Assert.NotNull(sample.EndScript);
            Assert.AreEqual("RunMeAfter.ps1", sample.EndScript.ScriptPath);
            Assert.AreEqual("ThisIsMe.txt", sample.EndScript.ScriptArguments);

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
