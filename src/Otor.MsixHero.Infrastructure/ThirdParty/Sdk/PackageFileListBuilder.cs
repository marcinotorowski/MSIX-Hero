using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Otor.MsixHero.Infrastructure.ThirdParty.Sdk
{
    public class PackageFileListBuilder
    {
        private readonly IDictionary<string, string> sourceFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly IList<SourceDirectory> sourceDirectories = new List<SourceDirectory>();

        public void AddFile(string sourceFile, string targetRelativeDirectory)
        {
            this.sourceFiles[targetRelativeDirectory] = sourceFile.Replace("/", "\\");
        }

        public void AddDirectory(string sourceDirectory, string wildcard, bool recursive, string targetRelativeDirectory)
        {
            this.sourceDirectories.Add(new SourceDirectory(sourceDirectory, targetRelativeDirectory, wildcard, recursive));
        }

        public void AddDirectory(string sourceDirectory, string wildcard, string targetRelativeDirectory)
        {
            this.AddDirectory(sourceDirectory, wildcard, false, targetRelativeDirectory);
        }

        public void AddDirectory(string sourceDirectory, bool recursive, string targetRelativeDirectory)
        {
            this.AddDirectory(sourceDirectory, "*", recursive, targetRelativeDirectory);
        }

        public void AddDirectory(string sourceDirectory, string targetRelativeDirectory)
        {
            this.AddDirectory(sourceDirectory, "*", false, targetRelativeDirectory);
        }

        public void AddManifest(string sourceManifestFilePath)
        {
            this.sourceFiles["AppxManifest.xml"] = sourceManifestFilePath;
        }

        /// <summary>
        /// Returns source path of the manifest file.
        /// </summary>
        /// <returns>The full source path to a manifest file.</returns>
        public string GetManifestSourcePath()
        {
            // ReSharper disable once StringLiteralTypo
            if (sourceFiles.TryGetValue("appxmanifest.xml", out var manifestPath))
            {
                return manifestPath;
            }

            foreach (var item in this.sourceDirectories.Where(sd => string.IsNullOrEmpty(sd.TargetRelativePath)))
            {
                // ReSharper disable once StringLiteralTypo
                var findManifestFiles = Directory.EnumerateFiles(item.SourcePath, "appxmanifest.xml", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (findManifestFiles != null)
                {
                    return findManifestFiles;
                }
            }

            return null;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("[Files]");
            stringBuilder.AppendLine();

            var targetRelativeFilePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var filesEnumeratedFromSourceDirectories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var directory in this.sourceDirectories)
            {
                foreach (var foundFile in Directory.EnumerateFiles(directory.SourcePath, directory.Wildcard, directory.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    if (!foundFile.StartsWith(directory.SourcePath, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var targetRelativePath = foundFile.Substring(directory.SourcePath.Length).TrimStart('\\');
                    if (!string.IsNullOrEmpty(directory.TargetRelativePath))
                    {
                        targetRelativePath = Path.Combine(directory.TargetRelativePath, targetRelativePath);
                    }

                    if (this.sourceFiles.ContainsKey(targetRelativePath))
                    {
                        continue;
                    }
                    
                    filesEnumeratedFromSourceDirectories.Add(targetRelativePath, foundFile);
                }
            }

            foreach (var item in filesEnumeratedFromSourceDirectories.Concat(this.sourceFiles))
            {
                var source = item.Value;
                var target = item.Key;

                switch (target.ToLowerInvariant())
                {
                    // ReSharper disable once StringLiteralTypo
                    case "appxblockmap.xml":
                        break;
                    // ReSharper disable once StringLiteralTypo
                    case "appxsignature.p7x":
                        break;
                    default:

                        if (!targetRelativeFilePaths.Add(target))
                        {
                            // File already added
                            continue;
                        }

                        stringBuilder.AppendLine($"\"{source}\"\t\"{target}\"");
                        break;
                }
            }

            return stringBuilder.ToString();
        }

        private struct SourceDirectory
        {
            public SourceDirectory(string sourcePath, string targetRelativePath, string wildcard, bool recursive)
            {
                SourcePath = sourcePath;
                TargetRelativePath = targetRelativePath;
                Wildcard = wildcard;
                Recursive = recursive;
            }

            public string SourcePath;
            public string TargetRelativePath;
            public string Wildcard;
            public bool Recursive;
        }
    }
}
