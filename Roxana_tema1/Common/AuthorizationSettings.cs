using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class AuthorizationSettings
    {
        public string Secret { get; set; } = string.Empty;

        public string Issuer {  get; set; } = string.Empty;

        public string Audience {  get; set; } = string.Empty;
        public int TokenLifetimeInMinutes { get; set; }
        public int IdTokenLifetimeInMinutes { get; set; }
        public int RefreshTokenLifetimeInMinutes { get; set; }
        public int AccessTokenLifetimeInMinutes { get; set; }
    }
}
