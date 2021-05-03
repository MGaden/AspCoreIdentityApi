using CoreIdentity.API.Helpers;
using CoreIdentity.API.Identity;
using CoreIdentity.API.Identity.Entities;
using CoreIdentity.API.Identity.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace CoreIdentity.API.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string tokenId);
        Task<VerifyTokenResponse> VerifyToken(TokenRequest tokenRequest);
        Task<bool> RevokeToken(string userId);
        bool RevokeTokenByToken(string token);
        object GetAllRefreshTokens();
    }

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly SecurityContext _securityContext;
        private readonly IConfiguration _configuration;

        public RefreshTokenService(SecurityContext securityContext, IConfiguration configuration)
        {
            _securityContext = securityContext;
            _configuration = configuration;
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string tokenId)
        {
            var refreshToken = new RefreshToken()
            {
                JwtId = tokenId,
                IsUsed = false,
                UserId = userId,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                IsRevoked = false,
                Token = HelperMethods.RandomString(25) + Guid.NewGuid()
            };

            //remove old tokens
            var oldTokens = _securityContext.RefreshTokens.Where(t => t.UserId == userId).ToList();
            if(oldTokens != null && oldTokens.Count > 0)
            {
                _securityContext.RemoveRange(oldTokens);
            }

            //save new one
            await _securityContext.RefreshTokens.AddAsync(refreshToken).ConfigureAwait(false);
            await _securityContext.SaveChangesAsync().ConfigureAwait(false);

            return refreshToken;
        }

        public object GetAllRefreshTokens()
        {
           return _securityContext.RefreshTokens.Select(token => new
            {
                token.Id,
                token.UserId,
                token.AddedDate
            });
        }

        public async Task<bool> RevokeToken(string userId)
        {
            try
            {
                var storedRefreshToken = _securityContext.RefreshTokens.Where(x => x.UserId == userId).ToList();

                if (storedRefreshToken != null)
                {
                     _securityContext.RemoveRange(storedRefreshToken);
                    await _securityContext.SaveChangesAsync().ConfigureAwait(false);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RevokeTokenByToken(string token)
        {
            try
            {
                var storedRefreshToken = _securityContext.RefreshTokens.Where(x => x.Token == token).ToList();

                if (storedRefreshToken != null)
                {
                    _securityContext.RemoveRange(storedRefreshToken);
                    _securityContext.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<VerifyTokenResponse> VerifyToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            VerifyTokenResponse response = new VerifyTokenResponse();

            try
            {
                // This validation function will make sure that the token meets the validation parameters
                // and its an actual jwt token not just a random string
                var _tokenValidationParameters = AuthenticationHelper.BuildTokenValidationParameters(_configuration["JwtSecurityToken:Issuer"], _configuration["JwtSecurityToken:Audience"], _configuration["JwtSecurityToken:Key"]);
                _tokenValidationParameters.ValidateLifetime = false;
                var principal = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

                // Now we need to check if the token has a valid security algorithm
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (result == false)
                    {
                        response.IsValid = false;
                        response.Error = "Your token is not valid!";
                        return response;
                    }
                }

                //validate token ip with cuurent request ip
                var ipClaim = principal.FindFirst(c => c.Type == "ip" && c.Value == tokenRequest.IpAddress);
                if (ipClaim == null)
                {
                    response.IsValid = false;
                    response.Error = "Your token is not valid!";
                    return response;
                }

                // Will get the time stamp in unix time
                var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                // we convert the expiry date from seconds to the date
                var expDate = HelperMethods.UnixTimeStampToDateTime(utcExpiryDate);

                if (expDate > DateTime.UtcNow)
                {
                    response.IsValid = false;
                    response.Error = "We cannot refresh this since the token has not expired!";
                    return response;
                }

                // Check the token we got if its saved in the db
                var storedRefreshToken = _securityContext.RefreshTokens.FirstOrDefault(x => x.Token == tokenRequest.RefreshToken);

                if (storedRefreshToken == null)
                {
                    response.IsValid = false;
                    response.Error = "refresh token doesnt exist!";
                    return response;
                }

                // Check the date of the saved token if it has expired
                if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
                {
                    response.IsValid = false;
                    response.Error = "token has expired, user needs to relogin!";
                    return response;
                }

                // check if the refresh token has been used
                if (storedRefreshToken.IsUsed)
                {
                    response.IsValid = false;
                    response.Error = "token has been used!";
                    return response;
                }

                // Check if the token is revoked
                if (storedRefreshToken.IsRevoked)
                {
                    response.IsValid = false;
                    response.Error = "token has been revoked!";
                    return response;
                }

                // we are getting here the jwt token id
                var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                // check the id that the recieved token has against the id saved in the db
                if (storedRefreshToken.JwtId != jti)
                {
                    response.IsValid = false;
                    response.Error = "the token doenst mateched the saved token!";
                    return response;
                }

                //will remove old one , not update isused
                //storedRefreshToken.IsUsed = true;
                //_securityContext.RefreshTokens.Update(storedRefreshToken);
                _securityContext.Remove(storedRefreshToken);

                await _securityContext.SaveChangesAsync().ConfigureAwait(false);

                response.IsValid = true;
                response.Error = "";
                response.UserId = storedRefreshToken.UserId;
                return response;
            }
            catch (Exception ex)
            {
                response.IsValid = false;
                response.Error = "Error has occured!";
                return response;
            }
        }

    }
}
