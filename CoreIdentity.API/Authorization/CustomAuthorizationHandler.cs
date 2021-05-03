using CoreIdentity.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoreIdentity.API.Authorization
{
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HasScopeHandler(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Issuer == requirement.Issuer))
                return Task.CompletedTask;

            // Find claim with scope
            var claim = context.User.FindFirst(c => c.Type == ClaimTypes.Role && c.Value == requirement.Scope);
            if (claim == null)
                return Task.CompletedTask;

            var userIp = IpHelper.GetUserIPAddress(_httpContextAccessor);
            if(!string.IsNullOrWhiteSpace(userIp))
            {
                var ipClaim = context.User.FindFirst(c => c.Type == "ip" && c.Value == userIp);
                if (ipClaim == null)
                    return Task.CompletedTask;
            }

            //// Succeed if the scope  contains the required scope
            //if (claim != null)
            //    context.Succeed(requirement);

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class HasScopeRequirement : IAuthorizationRequirement
    {
        public string Issuer { get; }
        public string Scope { get; }

        public HasScopeRequirement(string scope, string issuer)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        }
    }
}
