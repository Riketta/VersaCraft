using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.DirectoryServices;

namespace VersaCraft.Protocol
{
    public class CryptoUtils
    {
        private static readonly string VersaHashSalt = "A209F552";

        public static string CalculateFileMD5(string filepath)
        {
            using (MD5 md5 = MD5.Create())
            using (FileStream stream = File.OpenRead(filepath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string CalculateStringSHA1(string data)
        {
            using (SHA1 sha1 = SHA1.Create())
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data ?? "")))
            {
                byte[] hash = sha1.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Hash for storing in internal database and transporting.
        /// </summary>
        /// <param name="data">Expected to be password</param>
        /// <returns>SHA1(SHA1(password) + <see cref="VersaHashSalt"/>).</returns>
        public static string CalculateStringVersaHash(string data)
        {
            return CalculateStringSHA1(CalculateStringSHA1(data) + VersaHashSalt);
        }

        public static SecurityIdentifier GetComputerSid()
        {
            return new SecurityIdentifier((byte[])new DirectoryEntry(string.Format("WinNT://{0},Computer", Environment.MachineName)).Children.Cast<DirectoryEntry>().First().InvokeGet("objectSID"), 0).AccountDomainSid;
        }
    }
}