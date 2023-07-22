using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Entities
{
    public class UserClient
    {
        public string ApplicationUserId { get; set; }

        public int ClientId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}
