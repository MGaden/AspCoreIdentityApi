using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreIdentity.API.Identity.ViewModels
{
    public class ConfirmEmailViewModel
    {
        public string UserId { get; set; }
        public string Code { get; set; }
    }

    public class ConfirmEmailRequestViewModel
    {
        public string UserId { get; set; }

        public string Email { get; set; }

        public string ClientCallbackUrl { get; set; }
    }
}
