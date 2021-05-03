using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreIdentity.API.Constants
{
    public static class Claims
    {
        public const string ManageUsers = "Users:Manage";

        public const string ManageRoles = "Roles:Manage";

        public const string ManageUsersRoles = "UserRoles:Manage";
        public const string ManageRolesClaims = "RoleClaims:Manage";

        public static Dictionary<string, object> GetAllClaims()
        {
            var refs = typeof(Constants.Claims).GetFields()
                .Select(f => new { Key = f.Name, Value = f.GetValue(null) })
                .ToDictionary(item => item.Key, item => item.Value);

            return refs;
        }
    }
}
