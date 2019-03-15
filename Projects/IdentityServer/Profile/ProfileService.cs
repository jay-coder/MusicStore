using IdentityServer4.Models;
using IdentityServer4.Services;
using JayCoder.MusicStore.Core.Domain.Constants;
using JayCoder.MusicStore.Core.Domain.SQLEntities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JayCoder.MusicStore.Projects.IdentityServer.Profile
{
    public class ProfileService : IProfileService
    {
        protected UserManager<ApplicationUser> _userManager;

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = _userManager.GetUserAsync(context.Subject).Result;
            if (user != null)
            {
                var claims = new List<Claim>();
                claims.Add(new Claim(AuthorizeClaim.FirstName, string.IsNullOrEmpty(user.FirstName) ? string.Empty : user.FirstName));
                claims.Add(new Claim(AuthorizeClaim.LastName, string.IsNullOrEmpty(user.LastName) ? string.Empty : user.LastName));
                claims.Add(new Claim(AuthorizeClaim.UserType, user.UserType.ToString()));
                context.IssuedClaims.AddRange(claims);
            }
            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.FromResult(0);
        }
    }
}
