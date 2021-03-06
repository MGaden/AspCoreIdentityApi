using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using CoreIdentity.API.Identity.ViewModels;
using System.Collections.Generic;
using CoreIdentity.API.Services;

namespace CoreIdentity.API.Identity.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "resources:access")]
    [Produces("application/json")]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRefreshTokenService _refreshToken;

        public UserController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IRefreshTokenService refreshToken
            )
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._refreshToken = refreshToken;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<IdentityUser>), 200)]
        [Route("get")]
        public IActionResult Get() => Ok(
            _userManager.Users.Select(user => new
            {
                user.Id,
                user.Email,
                user.PhoneNumber,
                user.EmailConfirmed,
                user.LockoutEnabled,
                user.TwoFactorEnabled
            }));

        /// <summary>
        /// Get a user
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IdentityUser), 200)]
        [ProducesResponseType(typeof(IEnumerable<string>), 400)]
        [Route("get/{Id}")]
        public IActionResult Get(string Id)
        {
            if (String.IsNullOrEmpty(Id))
                return BadRequest(new string[] { "Empty parameter!" });

            return Ok(_userManager.Users
                .Where(user => user.Id == Id)
                .Select(user => new
                {
                    user.Id,
                    user.Email,
                    user.PhoneNumber,
                    user.EmailConfirmed,
                    user.LockoutEnabled,
                    user.TwoFactorEnabled
                })
                .FirstOrDefault());
        }

        /// <summary>
        /// Insert a user with an existing role
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        [Authorize(Constants.Claims.ManageUsers)]
        [HttpPost]
        [ProducesResponseType(typeof(IdentityResult), 200)]
        [ProducesResponseType(typeof(IEnumerable<string>), 400)]
        [Route("insertWithRole")]
        public async Task<IActionResult> Post([FromBody]UserViewModel model)
        {
            if (model == null)
                return BadRequest(new string[] { "No data in model!" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.Select(x => x.Errors.FirstOrDefault().ErrorMessage));

            IdentityUser user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = model.EmailConfirmed,
                PhoneNumber = model.PhoneNumber
            };

            IdentityRole role = await _roleManager.FindByIdAsync(model.RoleId).ConfigureAwait(false);
            if (role == null)
                return BadRequest(new string[] { "Could not find role!" });

            IdentityResult result = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);
            if (result.Succeeded)
            {
                IdentityResult result2 = await _userManager.AddToRoleAsync(user, role.Name).ConfigureAwait(false);
                if (result2.Succeeded)
                {
                    return Ok(new
                    {
                        user.Id,
                        user.Email,
                        user.PhoneNumber,
                        user.EmailConfirmed,
                        user.LockoutEnabled,
                        user.TwoFactorEnabled
                    });
                }
            }
            return BadRequest(result.Errors.Select(x => x.Description));
        }

        /// <summary>
        /// Update a user
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        [Authorize(Constants.Claims.ManageUsers)]
        [HttpPut]
        [ProducesResponseType(typeof(IdentityResult), 200)]
        [ProducesResponseType(typeof(IEnumerable<string>), 400)]
        [Route("update/{Id}")]
        public async Task<IActionResult> Put(string Id, [FromBody]EditUserViewModel model)
        {
            if (model == null)
                return BadRequest(new string[] { "No data in model!" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.Select(x => x.Errors.FirstOrDefault().ErrorMessage));

            IdentityUser user = await _userManager.FindByIdAsync(Id).ConfigureAwait(false);
            if (user == null)
                return BadRequest(new string[] { "Could not find user!" });

            // Add more fields to update
            user.Email = model.Email;
            //user.UserName = model.Email;
            user.EmailConfirmed = model.EmailConfirmed;
            user.PhoneNumber = model.PhoneNumber;
            user.LockoutEnabled = model.LockoutEnabled;
            user.TwoFactorEnabled = model.TwoFactorEnabled;

            IdentityResult result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest(result.Errors.Select(x => x.Description));
        }

        /// <summary>
        /// Delete a user (Will also delete link to roles)
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// 
        [Authorize(Constants.Claims.ManageUsers)]
        [HttpDelete]
        [ProducesResponseType(typeof(IdentityResult), 200)]
        [ProducesResponseType(typeof(IEnumerable<string>), 400)]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(string Id)
        {
            if (String.IsNullOrEmpty(Id))
                return BadRequest(new string[] { "Empty parameter!" });

            if (Id == Constants.DefaultUser.Id)
                return BadRequest(new string[] { "Can't delete predefined users!" });

            IdentityUser user = await _userManager.FindByIdAsync(Id).ConfigureAwait(false);
            if (user == null)
                return BadRequest(new string[] { "Could not find user!" });

            IdentityResult result = await _userManager.DeleteAsync(user).ConfigureAwait(false);
            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest(result.Errors.Select(x => x.Description));
        }

        /// <summary>
        /// Revoke a refrsh token from user
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// 
        [Authorize(Constants.Claims.ManageUsers)]
        [HttpDelete]
        [Route("revokeToken/{Id}")]
        public async Task<IActionResult> RevokeToken(string Id)
        {
            bool result = await _refreshToken.RevokeToken(Id).ConfigureAwait(false);
            if(result)
            return Ok(new { message = "Token revoked" });
            else
                return BadRequest(new string[] { "Failed to revoke token!" });

        }

        /// <summary>
        /// Get all refresh tokens
        /// </summary>
        /// <returns></returns>
        /// 
        [Authorize(Constants.Claims.ManageUsers)]
        [HttpGet]
        [Route("getAllRefreshTokens")]
        public IActionResult GetAllRefreshTokens() => Ok(
           _refreshToken.GetAllRefreshTokens()
            );
    }
}
