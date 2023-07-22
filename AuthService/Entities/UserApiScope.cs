using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Entities
{
    public class UserApiScope
    {
        public string ApplicationUserId { get; set; }

        public int ApiScopeId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}
