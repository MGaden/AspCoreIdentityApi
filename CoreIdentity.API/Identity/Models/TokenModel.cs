using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoreIdentity.API.Identity.Models
{
    public class TokenModel
    {
        [JsonIgnore]
        public bool? HasVerifiedEmail { get; set; }

        [JsonIgnore]
        public bool? TFAEnabled { get; set; }
        public string Token { get; set; }

        public string RefreshToken { get; set; }
    }

    public class TokenRequest
    {
        [Required]
        public string Token { get; set; }
        //[Required]
        public string RefreshToken { get; set; }

        [JsonIgnore]
        public string IpAddress { get; set; }
    }

    public class VerifyTokenResponse
    {
        public bool IsValid { get; set; }
        public string Error { get; set; }

        public string UserId { get; set; }
    }

    public class RevokeTokenRequest
    {
        public string Token { get; set; }
    }

}
