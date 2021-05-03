using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreIdentity.API.Identity.Models
{
    public class UserModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get;  set; }
        public IList<string> Roles { get;  set; }
    }
}
