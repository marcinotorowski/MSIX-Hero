using CommandLine;

namespace Otor.MsixHero.Cli.Verbs
{
    public abstract class BaseVerb
    {
        public string ToCommandLineString(bool withExeName = true)
        {
            if (withExeName)
            {
                return "msixherocli.exe " + Parser.Default.FormatCommandLine(this);
            }

            return Parser.Default.FormatCommandLine(this);
        }
    }
}