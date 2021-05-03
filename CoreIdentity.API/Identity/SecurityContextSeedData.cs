using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoreIdentity.API.Identity
{
    public static class SecurityContextSeedData
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            //Seeding a  'Super Admin' role to AspNetRoles table
            var superAdminRole = new IdentityRole { Id = Constants.DefaultRole.Id, Name = Constants.DefaultRole.SuperAdmin, NormalizedName = Constants.DefaultRole.SuperAdmin.ToUpper() };
            modelBuilder.Entity<IdentityRole>().HasData(superAdminRole);

            var allClaims = Constants.Claims.GetAllClaims();
            if (allClaims != null && allClaims.Keys.Count > 0)
            {
                int Id = 0;
                foreach (string key in allClaims.Keys)
                {
                    Id++;
                    modelBuilder.Entity<IdentityRoleClaim<string>>().HasData(new IdentityRoleClaim<string> { Id = Id , RoleId = superAdminRole.Id, ClaimType = ClaimTypes.Role, ClaimValue = allClaims[key].ToString() });
                }

            }

            //a hasher to hash the password before seeding the user to the db
            var hasher = new PasswordHasher<IdentityUser>();

            var superAdminUser = new IdentityUser
            {
                Id = Constants.DefaultUser.Id,
                UserName = Constants.DefaultUser.UserName,
                Email = Constants.DefaultUser.Email,
                NormalizedEmail = Constants.DefaultUser.Email.ToUpper(),
                TwoFactorEnabled = false,
                EmailConfirmed = true,
                NormalizedUserName = Constants.DefaultUser.UserName.ToUpper(),
                PasswordHash = hasher.HashPassword(null, Constants.DefaultUser.Password)
            };

            //Seeding the User to AspNetUsers table
            modelBuilder.Entity<IdentityUser>().HasData(superAdminUser);


            //Seeding the relation between our user and role to AspNetUserRoles table
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = superAdminRole.Id,
                    UserId = superAdminUser.Id
                }
            );

        }
    }
}
