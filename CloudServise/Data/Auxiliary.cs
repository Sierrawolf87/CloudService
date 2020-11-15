using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CloudService_API.Data
{
    public static class Auxiliary
    {
        public static string GenerateHashPassword(string password)
        {
            var sSourceData = "CloudServise";
            // generate a 128-bit salt using a secure PRNG
            byte[] salt = Encoding.ASCII.GetBytes(sSourceData);
            
            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        public static string GetExtension(string path)
        {
            var split = path.Split('.');
            return $".{split.Last()}";
        }
    }
}
