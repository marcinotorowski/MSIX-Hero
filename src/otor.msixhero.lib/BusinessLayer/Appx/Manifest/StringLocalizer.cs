using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest
{
    public static class StringLocalizer
    {
        public static string Localize(string priFile, string appId, string resourceId)
        {
            if (resourceId == null || !resourceId.StartsWith("ms-resource:"))
            {
                return resourceId;
            }

            if (!File.Exists(priFile))
            {
                return resourceId;
            }

            string fullString;

            resourceId = resourceId.Remove(0, "ms-resource:".Length);
            if (string.IsNullOrEmpty(resourceId))
            {
                return resourceId;
            }

            if (resourceId[0] == '/')
            {
                var split = resourceId.Split('/');
                var newResourceId = string.Join('/', split.Take(2)) + "/" + string.Join('.', split.Skip(2));
                fullString = "@{" + priFile + "?ms-resource://" + appId + newResourceId.TrimEnd('/') + "}";
            }
            else if (resourceId.IndexOf('/') != -1)
            {
                var split = resourceId.Split('/');
                var newResourceId = string.Join('/', split.Take(1)) + "/" + string.Join('.', split.Skip(1));
                fullString = "@{" + priFile + "?ms-resource://" + appId + "/" + newResourceId.TrimEnd('/') + "}";
            }
            else
            {
                var split = resourceId.Split('/');
                var newResourceId = string.Join('/', split.Take(1)) + "/" + string.Join('.', split.Skip(1));
                fullString = "@{" + priFile + "?ms-resource://" + appId + "/resources/" + newResourceId.TrimEnd('/') + "}";
            }

            var outBuff = new StringBuilder(1024);
            if (SHLoadIndirectString(fullString, outBuff, outBuff.Capacity, IntPtr.Zero) == 0)
            {
                return outBuff.ToString();
            }
            else
            {
                return resourceId;
            }
        }

        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        private static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);
    }
}
