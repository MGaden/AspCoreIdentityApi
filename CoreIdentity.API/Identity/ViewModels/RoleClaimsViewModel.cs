using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreIdentity.API.Identity.ViewModels
{
    public class RoleClaimsViewModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public List<string> Claims { get; set; }
    }
}
