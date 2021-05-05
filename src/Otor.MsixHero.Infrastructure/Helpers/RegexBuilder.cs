using System.Text;
using System.Text.RegularExpressions;

namespace Otor.MsixHero.Infrastructure.Helpers
{
    public static class RegexBuilder
    {
        public static Regex FromWildcard(string wildcard)
        { 
            if (wildcard == "*")
            {
                return new Regex(".*");
            }

            var sb = new StringBuilder();

            var index = 0;
            var special = new[] {'?', '*'};

            if (wildcard.Length == 0 || wildcard[0] != '*')
            {
                sb.Append('^');
            }

            while (index < wildcard.Length)
            {
                var findNextSpecial = wildcard.IndexOfAny(special, index + 1);
                if (findNextSpecial == -1)
                {
                    sb.Append(Regex.Escape(wildcard.Substring(index)));
                    break;
                }
                else
                {
                    sb.Append(wildcard.Substring(index, findNextSpecial - index));
                    switch (wildcard[findNextSpecial])
                    {
                        case '?':
                            sb.Append(".");
                            break;
                        case '*':
                            sb.Append(".*");
                            break;
                    }

                    index = findNextSpecial;
                }

                index++;
            }

            if (wildcard.Length == 0 || wildcard[^1] != '*')
            {
                sb.Append('$');
            }

            return new Regex(sb.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}
