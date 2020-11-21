using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudService_API.Data
{
    public class PasswordHashSettings
    {
        public string HashKey { get; set; }

        public PasswordHashSettings() {}

        public PasswordHashSettings(string hashKey)
        {
            HashKey = string.IsNullOrEmpty(hashKey) ? "CloudService" : hashKey;
        }
    }
}
