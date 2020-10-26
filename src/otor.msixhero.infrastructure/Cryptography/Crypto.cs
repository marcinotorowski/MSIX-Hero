using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace Otor.MsixHero.Infrastructure.Cryptography
{
    public class Crypto
    {
        // ReSharper disable once InconsistentNaming
        private static readonly byte[] salt;

        static Crypto()
        {
            var baseString = "MSIX-Hero:" + Environment.UserDomainName + "\\" + Environment.UserName;

            using (var md5 = MD5.Create())
            {
                salt = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(baseString.ToLowerInvariant()));
            }
        }

        public string Protect(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
            {
                return plainText;
            }

            var data = System.Text.Encoding.UTF8.GetBytes(plainText);
            var protectedData = ProtectedData.Protect(data, new byte[] { 0x61, 0x73, 0x64, 0x61, 0x6a, 0xc4, 0x85, 0xc5, 0x82, 0x7a, 0x64, 0x61, 0x73, 0x64, 0x40, 0x23, 0x61, 0x64, 0x25, 0x25, 0x25, 0x40, 0x24, 0x23, 0x21, 0x40, 0x34, 0x31, 0x31, 0x24, 0x25, 0x24, 0x23, 0x5e, 0x23, 0x73, 0x64, 0x61, 0x63, 0x62, 0x76, 0x76, 0x61, 0x64, 0x73, 0x61, 0x64, 0x61, 0x72, 0x77, 0x61 }, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedData);
        }

        public string Protect(SecureString plainText)
        {
            if (plainText == null || plainText.Length == 0)
            {
                return string.Empty;
            }

            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(plainText);
                // ReSharper disable once AssignNullToNotNullAttribute
                var data = System.Text.Encoding.UTF8.GetBytes(Marshal.PtrToStringUni(unmanagedString));
                var protectedData = ProtectedData.Protect(data, new byte[] { 0x61, 0x73, 0x64, 0x61, 0x6a, 0xc4, 0x85, 0xc5, 0x82, 0x7a, 0x64, 0x61, 0x73, 0x64, 0x40, 0x23, 0x61, 0x64, 0x25, 0x25, 0x25, 0x40, 0x24, 0x23, 0x21, 0x40, 0x34, 0x31, 0x31, 0x24, 0x25, 0x24, 0x23, 0x5e, 0x23, 0x73, 0x64, 0x61, 0x63, 0x62, 0x76, 0x76, 0x61, 0x64, 0x73, 0x61, 0x64, 0x61, 0x72, 0x77, 0x61 }, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(protectedData);
            }
            finally
            {
                if (unmanagedString != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
                }
            }
            
        }

        public SecureString Unprotect(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
            {
                return new SecureString();
            }

            var protectedData = Convert.FromBase64String(encryptedText);
            var data = ProtectedData.Unprotect(protectedData, new byte[] { 0x61, 0x73, 0x64, 0x61, 0x6a, 0xc4, 0x85, 0xc5, 0x82, 0x7a, 0x64, 0x61, 0x73, 0x64, 0x40, 0x23, 0x61, 0x64, 0x25, 0x25, 0x25, 0x40, 0x24, 0x23, 0x21, 0x40, 0x34, 0x31, 0x31, 0x24, 0x25, 0x24, 0x23, 0x5e, 0x23, 0x73, 0x64, 0x61, 0x63, 0x62, 0x76, 0x76, 0x61, 0x64, 0x73, 0x61, 0x64, 0x61, 0x72, 0x77, 0x61 }, DataProtectionScope.CurrentUser);
            
            var sec = new SecureString();
            foreach (var c in System.Text.Encoding.UTF8.GetString(data))
            {
                sec.AppendChar(c);
            }

            return sec;
        }
        
        public string EncryptString(string plainText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentNullException("plainText");
            }

            if (string.IsNullOrEmpty(sharedSecret))
            {
                throw new ArgumentNullException("sharedSecret");
            }

            RijndaelManaged aesAlg = null;

            try
            {
                using (var key = new Rfc2898DeriveBytes(sharedSecret, salt))
                {
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                    using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        using (var msEncrypt = new MemoryStream())
                        {
                            // prepend the IV
                            msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                            msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                            {
                                using (var swEncrypt = new StreamWriter(csEncrypt))
                                {
                                    swEncrypt.Write(plainText);
                                }
                            }

                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            finally
            {
                if (aesAlg != null)
                {
                    aesAlg.Clear();
                    aesAlg.Dispose();
                }
            }
        }

        public string DecryptString(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                throw new ArgumentNullException("cipherText");
            }

            if (string.IsNullOrEmpty(sharedSecret))
            {
                throw new ArgumentNullException("sharedSecret");
            }

            RijndaelManaged aesAlg = null;
            
            try
            {
                using (var key = new Rfc2898DeriveBytes(sharedSecret, salt))
                {
                    var bytes = Convert.FromBase64String(cipherText);

                    using (var msDecrypt = new MemoryStream(bytes))
                    {
                        aesAlg = new RijndaelManaged();
                        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                        aesAlg.IV = ReadByteArray(msDecrypt);
                        using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                        {
                            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (var srDecrypt = new StreamReader(csDecrypt))
                                {
                                    return srDecrypt.ReadToEnd();
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (aesAlg != null)
                {
                    aesAlg.Clear();
                    aesAlg.Dispose();
                }
            }
        }

        private static byte[] ReadByteArray(Stream s)
        {
            var rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }
    }
}
