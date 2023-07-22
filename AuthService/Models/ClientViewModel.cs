using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AuthService.Models
{
    public class ClientViewModel
    {
        public int Id { get; set; }

        public string ClientName { get; set; }

        public string ClientSecrets { get; set; }

        public string AllowedGrantTypes { get; set; }

        public string RedirectUris { get; set; }

        public string PostLogoutRedirectUris { get; set; }

        public string AllowedCorsOrigins { get; set; }

        public string AllowedScopes { get; set; }

        public int AccessTokenLifetime { get; set; }

        public bool RequireClientSecret { get; set; }

        public bool RequirePkce { get; set; }

        public bool AllowRememberConsent { get; set; }

        public bool AllowOfflineAccess { get; set; }

        public bool AllowAccessTokensViaBrowser { get; set; }
    }
}
