using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MD5_ = System.Security.Cryptography.MD5;
using SHA256_ = System.Security.Cryptography.SHA256;

namespace WebApp.Utils
{
    public class Hash
    {
        private static string DoHash(HashAlgorithm Algo, ref string InString)
        {
            return string.Join("",
                from ba in Algo.ComputeHash(Encoding.UTF8.GetBytes(InString))
                select ba.ToString("x2")).ToLower();
        }

        /// <summary>
        /// Calculates MD5 Hash.
        /// </summary>
        /// <param name="InString"></param>
        /// <returns></returns>
        public static string MD5(string InString)
        {
            using (MD5_ md5 = MD5_.Create())
                return DoHash(md5, ref InString);
        }

        /// <summary>
        /// Calculates SHA256 Hash.
        /// </summary>
        /// <param name="InString"></param>
        /// <returns></returns>
        public static string SHA256(string InString)
        {
            using (SHA256_ sha256 = SHA256_.Create())
                return DoHash(sha256, ref InString);
        }
    }
}
