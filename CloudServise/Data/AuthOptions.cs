using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace CloudService_API.Data
{
    public class AuthOptions
    {
        public const string ISSUER = "CloudService"; // издатель токена
        public const string AUDIENCE = "CloudServiceClient"; // потребитель токена
        const string KEY = "CloudServiceTokenKey";   // ключ для шифрации
        public const int LIFETIME = 60*2; // время жизни токена в минутах
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
