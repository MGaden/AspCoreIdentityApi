using CoreIdentity.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreIdentity.API.Helpers
{
    public class AuthorizationHelper
    {
        public static void ConfigureService(IServiceCollection services)
        {
            services.AddAuthorization();

            // register the scope authorization handler
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
        }
    }
}
