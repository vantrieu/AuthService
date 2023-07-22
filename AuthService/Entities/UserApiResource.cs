using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Entities
{
    public class UserApiResource
    {
        public string ApplicationUserId { get; set; }

        public int ApiResourceId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}
