﻿using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using AuthService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthService.IdentityServer
{
    public class CustomProfileService : IProfileService
    {
        private readonly ILogger<CustomProfileService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CustomProfileService(UserManager<ApplicationUser> userManager,
            ILogger<CustomProfileService> logger, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject?.GetSubjectId();
            if (sub == null)
            {
                throw new Exception("No sub claim present");
            }

            var user = await _userManager.FindByIdAsync(sub);
            if (user == null)
            {
                _logger.LogWarning("No user found matching subject Id: {sub}", sub);
            }
            else
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                    new Claim("fullName", $"{user.FirstName} {user.LastName}"),
                    new Claim(JwtClaimTypes.Email, user.Email)
                };

                var userRoleNames = await _userManager.GetRolesAsync(user);
                foreach (var userRole in userRoleNames)
                {
                    claims.Add(new Claim(JwtClaimTypes.Role, userRole));
                }

                var userRoles = await _roleManager.Roles.Where(x => userRoleNames.Contains(x.Name)).ToListAsync();
                foreach (var role in userRoles)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    var permissions = roleClaims.Where(x => x.Type == "Permission").Select(x => x.Value);
                    if (permissions.Any())
                    {
                        foreach (var permission in permissions)
                        {
                            claims.Add(new Claim("permissions", permission));
                        }
                    }
                }

                context.IssuedClaims.AddRange(claims);
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject?.GetSubjectId();
            if (sub == null)
            {
                throw new Exception("No subject Id claim present");
            }

            var user = await _userManager.FindByIdAsync(sub);
            if (user == null)
            {
                _logger.LogWarning("No user found matching subject Id: {sub}", sub);
            }

            context.IsActive = user != null;
        }
    }
}
