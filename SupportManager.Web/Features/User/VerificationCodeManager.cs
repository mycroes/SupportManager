using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SupportManager.Web.Features.User
{
    public static class VerificationCodeManager
    {
        private static readonly Random global = new Random();
        [ThreadStatic]
        private static Random local;

        private static Random LocalRandom
        {
            get
            {
                if (local != null) return local;

                int seed;
                lock (global) seed = global.Next();
                local = new Random(seed);
                return local;
            }
        }

        public static string GenerateCode()
        {
            return LocalRandom.Next(999999).ToString("D6");
        }

        public static string GetHash(string input)
        {
            var sha256 = new SHA256Managed();
            var result = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

            return string.Concat(result.Select(x => x.ToString("x2")));
        }
    }
}