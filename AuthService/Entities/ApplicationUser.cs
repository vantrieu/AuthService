using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace AuthService.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() : base()
        {
        }

        public ApplicationUser(string userName) : base(userName)
        {
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Avatar { get; set; }

        public DateTime DateOfBirth { get; set; }

        public ICollection<UserClient> UserClients { get; set; }

        public ICollection<UserIdentityResource> UserIdentityResources { get; set; }

        public ICollection<UserApiScope> UserApiScopes { get; set; }

        public ICollection<UserApiResource> UserApiResources { get; set; }
    }
}
