using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DTOs
{
    public class AuthenticateResponseDto
    {
        public string IdToken { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        
    }
}
