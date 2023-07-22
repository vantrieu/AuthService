using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Entities
{
    public class UserIdentityResource
    {
        public string ApplicationUserId { get; set; }

        public int IdentityResourceId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}
