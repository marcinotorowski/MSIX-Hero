using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Otor.MsixHero.Appx.Reader.Helpers
{
    internal static class ZipFileChecker
    {
        public static bool IsZipFile(string filePath)
        {
            // ZIP file signature (first 4 bytes of the file)
            var zipSignature = new byte[] { 0x50, 0x4B, 0x03, 0x04 };

            var fileHeader = new byte[4];

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var _ = fs.Read(fileHeader, 0, fileHeader.Length);
                }

                return zipSignature.SequenceEqual(fileHeader);
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static async Task<bool> IsZipFileAsync(string filePath)
        {
            // ZIP file signature (first 4 bytes of the file)
            var zipSignature = new byte[] { 0x50, 0x4B, 0x03, 0x04 };

            var fileHeader = new byte[4];

            try
            {
                await using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var _ = await fs.ReadAsync(fileHeader, 0, fileHeader.Length).ConfigureAwait(false);
                }

                return zipSignature.SequenceEqual(fileHeader);
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
