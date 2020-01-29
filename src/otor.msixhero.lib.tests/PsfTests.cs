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
                                    ""base"": ""ClearbyteMSIXSample/"",
                                    ""patterns"": [
                                    "".*\\.json""
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
