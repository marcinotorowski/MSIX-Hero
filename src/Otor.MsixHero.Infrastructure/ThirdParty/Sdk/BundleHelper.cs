using System;
using System.IO;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public static class BundleHelper
    {
        static BundleHelper()
        {
            SdkPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redistr", "sdk");
            MsixMgrPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redistr", "msixmgr");
            PsfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "redistr", "psf");
            TemplatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates");
        }

        public static string SdkPath { get; set; }

        public static string MsixMgrPath { get; set; }

        public static string PsfPath { get; set; }

        public static string TemplatesPath { get; set; }
    }
}
