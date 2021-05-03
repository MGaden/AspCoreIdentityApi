using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using CoreIdentity.API.Identity.ViewModels;
using System.Collections.Generic;
using System.Security.Claims;
using CoreIdentity.API.Helpers;

namespace CoreIdentity.API.Identity.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "resources:access")]
    [Produces("application/json")]
    [Route("api/roleClaims")]
    public class RoleClaimsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleClaimsController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        /// <summary>
        /// Get all claims
        /// </summary>
        /// <returns></returns>
        /// 
        [Authorize(Constants.Claims.ManageRolesClaims)]
        [HttpGet]
        [Route("getAllClaims")]
        public object GetAllClaims()
        {
            //return Constants.Claims.GetAllClaims() ;
            return JsonHelper.ReadJsonFile(@"Constants/claims.json");
        }
        /// <summary>
        /// Get a role claims
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// 
        [Authorize(Constants.Claims.ManageRolesClaims)]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [Route("get/{Id}")]
        public async Task<IActionResult> Get(string Id)
        {
            var role = await _roleManager.FindByIdAsync(Id).ConfigureAwait(false);

            if (role == null)
                return BadRequest(new string[] { "Could not find role!" });

            var claims = await _roleManager.GetClaimsAsync(role).ConfigureAwait(false);
            if(claims != null && claims.Count > 0)
            {
                return Ok(claims.Select(c => c.Value).ToList());
            }
            return Ok();
        }

        /// <summary>
        /// Add a role to claim
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        [Authorize(Constants.Claims.ManageRolesClaims)]
        [HttpPost]
        [ProducesResponseType(typeof(IdentityResult), 200)]
        [ProducesResponseType(typeof(IEnumerable<string>), 400)]
        [Route("add")]
        public async Task<IActionResult> Post([FromBody] RoleClaimsViewModel model)
        {
            if (model == null)
                return BadRequest(new string[] { "No data in model!" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.Select(x => x.Errors.FirstOrDefault().ErrorMessage));

            IdentityRole role = await _roleManager.FindByIdAsync(model.Id).ConfigureAwait(false);
            if (role == null)
                return BadRequest(new string[] { "Could not find role!" });

            if(model.Id == Constants.DefaultRole.Id)
            {
                if(model.Claims == null || model.Claims.Count < Constants.Claims.GetAllClaims().Keys.Count)
                    return BadRequest(new string[] { "Could not delete predefined claims!" });
            }

            var claims = await _roleManager.GetClaimsAsync(role).ConfigureAwait(false);

            if (claims != null && claims.Count > 0)
            {
                foreach (var claim in claims)
                {
                    await _roleManager.RemoveClaimAsync(role, claim).ConfigureAwait(false);
                }
            }

            if (model.Claims != null && model.Claims.Count > 0)
            {
                foreach (string rowClaim in model.Claims)
                {
                    await _roleManager.AddClaimAsync(role, new Claim(ClaimTypes.Role, rowClaim)).ConfigureAwait(false);
                }

            }

            return Ok();
        }


    }
}
