using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using AuthService.Data;
using AuthService.Entities;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static IdentityServer4.Models.IdentityResources;

namespace AuthService.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        private readonly ConfigurationDbContext _configurationDbContext;

        private readonly ICurrentUserService _currentUserService;

        private readonly List<string> _defalutScopes = new List<string> { "openid", "profile", "email", "offline_access" };

        public ClientController(ApplicationDbContext applicationDbContext, ICurrentUserService currentUserService,
            ConfigurationDbContext configurationDbContext)
        {
            _applicationDbContext = applicationDbContext;
            _currentUserService = currentUserService;
            _configurationDbContext = configurationDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var clientIds = await _applicationDbContext.UserClients.Where(x => x.ApplicationUserId == _currentUserService.UserId)
                .Select(x => x.ClientId).ToListAsync();

            var client = await _configurationDbContext.Clients.Where(x => clientIds.Contains(x.Id)).Include(x => x.RedirectUris)
                .Include(x => x.PostLogoutRedirectUris).Include(x => x.AllowedCorsOrigins).ToListAsync();

            return View(client);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientViewModel client)
        {
            var isExistedClient = await _configurationDbContext.Clients.AnyAsync(x => x.ClientId.Equals(client.ClientName)
                || x.ClientName.Equals(client.ClientName));

            if (isExistedClient)
            {
                ModelState.AddModelError(string.Empty, "Client Name already existed, please try again.");

                return View(client);
            }

            var clientModel = new Client
            {
                ClientId = client.ClientName,
                ClientName = client.ClientName,
                ClientSecrets = { new Secret(client.ClientSecrets.Sha256()) },
                AllowedGrantTypes = client.AllowedGrantTypes?.Split(" ").ToList(),
                RedirectUris = client.RedirectUris?.Split(" ").ToList(),
                PostLogoutRedirectUris = client.PostLogoutRedirectUris?.Split(" ").ToList(),
                AllowedCorsOrigins = client.AllowedCorsOrigins?.Split(" ").ToList(),
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.OfflineAccess
                },
                AccessTokenLifetime = client.AccessTokenLifetime,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                RequireClientSecret = client.RequireClientSecret,
                RequirePkce = client.RequirePkce,
                AllowRememberConsent = client.AllowRememberConsent,
                AllowOfflineAccess = client.AllowOfflineAccess,
                AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser
            };

            clientModel.AllowedScopes.AddRange(client.AllowedScopes?.Split(" ").ToList());

            var newClient = await _configurationDbContext.Clients.AddAsync(clientModel.ToEntity());

            await CreateApiScopeAsync(client.AllowedScopes);

            await _configurationDbContext.SaveChangesAsync();

            await _applicationDbContext.UserClients.AddAsync(new UserClient
            {
                ApplicationUserId = _currentUserService.UserId,
                ClientId = newClient.Entity.Id
            });

            await _applicationDbContext.SaveChangesAsync();

            return Redirect("/Client/Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _configurationDbContext.Clients.Where(x => x.Id == id).Include(x => x.RedirectUris)
                .Include(x => x.PostLogoutRedirectUris).Include(x => x.AllowedCorsOrigins)
                .Include(x => x.AllowedScopes).Select(x => new ClientViewModel
                {
                    Id = x.Id,
                    ClientName = x.ClientName,
                    ClientSecrets = string.Join(" ", x.ClientSecrets.Select(x => x.Value)),
                    AllowedGrantTypes = string.Join(" ", x.AllowedGrantTypes.Select(x => x.GrantType).ToList()),
                    RedirectUris = string.Join(" ", x.RedirectUris.Select(x => x.RedirectUri).ToList()),
                    PostLogoutRedirectUris = string.Join(" ", x.PostLogoutRedirectUris.Select(x => x.PostLogoutRedirectUri).ToList()),
                    AllowedCorsOrigins = string.Join(" ", x.AllowedCorsOrigins.Select(x => x.Origin).ToList()),
                    AllowedScopes = string.Join(" ", x.AllowedScopes.Where(x => !_defalutScopes.Contains(x.Scope)).Select(x => x.Scope).ToList()),
                    AccessTokenLifetime = x.AccessTokenLifetime,
                    RequireClientSecret = x.RequireClientSecret,
                    RequirePkce = x.RequirePkce,
                    AllowRememberConsent = x.AllowRememberConsent,
                    AllowOfflineAccess = x.AllowOfflineAccess,
                    AllowAccessTokensViaBrowser = x.AllowAccessTokensViaBrowser,
                }).FirstOrDefaultAsync();

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientViewModel clientVM)
        {
            if (ModelState.IsValid)
            {
                //delete old client
                var client = await _configurationDbContext.Clients.Where(x => x.Id == clientVM.Id).Include(x => x.RedirectUris)
                .Include(x => x.PostLogoutRedirectUris).Include(x => x.AllowedCorsOrigins)
                .Include(x => x.AllowedScopes).FirstOrDefaultAsync();

                _configurationDbContext.Clients.Remove(client);

                await _configurationDbContext.SaveChangesAsync();

                var deleteData = await _applicationDbContext.UserClients.Where(x => x.ClientId == clientVM.Id).FirstOrDefaultAsync();

                _applicationDbContext.UserClients.Remove(deleteData);

                await _applicationDbContext.SaveChangesAsync();

                //add new client
                var isExistedClient = await _configurationDbContext.Clients.AnyAsync(x => x.ClientId.Equals(client.ClientName)
                || x.ClientName.Equals(client.ClientName));

                if (isExistedClient)
                {
                    ModelState.AddModelError(string.Empty, "Client Name already existed, please try again.");

                    return View(clientVM);
                }

                var clientModel = new Client
                {
                    ClientId = clientVM.ClientName,
                    ClientName = clientVM.ClientName,
                    ClientSecrets = { new Secret(clientVM.ClientSecrets.Sha256()) },
                    AllowedGrantTypes = clientVM.AllowedGrantTypes?.Split(" ").ToList(),
                    RedirectUris = clientVM.RedirectUris?.Split(" ").ToList(),
                    PostLogoutRedirectUris = clientVM.PostLogoutRedirectUris?.Split(" ").ToList(),
                    AllowedCorsOrigins = clientVM.AllowedCorsOrigins?.Split(" ").ToList(),
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess
                    },
                    AccessTokenLifetime = clientVM.AccessTokenLifetime,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    RequireClientSecret = clientVM.RequireClientSecret,
                    RequirePkce = clientVM.RequirePkce,
                    AllowRememberConsent = clientVM.AllowRememberConsent,
                    AllowOfflineAccess = clientVM.AllowOfflineAccess,
                    AllowAccessTokensViaBrowser = clientVM.AllowAccessTokensViaBrowser
                };

                clientModel.AllowedScopes.AddRange(clientVM.AllowedScopes?.Split(" ").ToList());

                var newClient = await _configurationDbContext.Clients.AddAsync(clientModel.ToEntity());

                await CreateApiScopeAsync(clientVM.AllowedScopes);

                //save change data
                await _configurationDbContext.SaveChangesAsync();

                await _applicationDbContext.UserClients.AddAsync(new UserClient
                {
                    ApplicationUserId = _currentUserService.UserId,
                    ClientId = newClient.Entity.Id
                });

                await _applicationDbContext.SaveChangesAsync();

                TempData["ResultOk"] = "Data Updated Successfully !";

                return RedirectToAction(nameof(Index));
            }

            return View(clientVM);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _configurationDbContext.Clients.Where(x => x.Id == id).Include(x => x.RedirectUris)
                .Include(x => x.PostLogoutRedirectUris).Include(x => x.AllowedCorsOrigins)
                .Include(x => x.AllowedScopes).Select(x => new ClientViewModel
                {
                    Id = x.Id,
                    ClientName = x.ClientName,
                    ClientSecrets = string.Join(" ", x.ClientSecrets.Select(x => x.Value)),
                    AllowedGrantTypes = string.Join(" ", x.AllowedGrantTypes.Select(x => x.GrantType).ToList()),
                    RedirectUris = string.Join(" ", x.RedirectUris.Select(x => x.RedirectUri).ToList()),
                    PostLogoutRedirectUris = string.Join(" ", x.PostLogoutRedirectUris.Select(x => x.PostLogoutRedirectUri).ToList()),
                    AllowedCorsOrigins = string.Join(" ", x.AllowedCorsOrigins.Select(x => x.Origin).ToList()),
                    AllowedScopes = string.Join(" ", x.AllowedScopes.Select(x => x.Scope).ToList()),
                    AccessTokenLifetime = x.AccessTokenLifetime,
                    RequireClientSecret = x.RequireClientSecret,
                    RequirePkce = x.RequirePkce,
                    AllowRememberConsent = x.AllowRememberConsent,
                    AllowOfflineAccess = x.AllowOfflineAccess,
                    AllowAccessTokensViaBrowser = x.AllowAccessTokensViaBrowser,
                }).FirstOrDefaultAsync();

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var deleteRecord = await _applicationDbContext.UserClients.Where(x => x.ClientId == id).FirstOrDefaultAsync();

            if (deleteRecord == null)
            {
                return NotFound();
            }
            _applicationDbContext.UserClients.Remove(deleteRecord);

            await _applicationDbContext.SaveChangesAsync();

            var client = await _configurationDbContext.Clients.Where(x => x.Id == id).FirstOrDefaultAsync();

            _configurationDbContext.Clients.Remove(client);

            await _configurationDbContext.SaveChangesAsync();

            TempData["ResultOk"] = "Data Deleted Successfully !";

            return RedirectToAction(nameof(Index));
        }

        private async Task CreateApiScopeAsync(string scopeName)
        {
            var scopes = scopeName.Split(" ").ToList();

            var apiScopes = await _configurationDbContext.ApiScopes.Where(x => scopes.Contains(x.Name)).Select(x => x.Name).ToListAsync();

            if (apiScopes.Any())
            {
                scopes = scopes.Except(apiScopes).ToList();
            }

            if (!scopes.Any())
            {
                return;
            }

            var newApiScopes = new List<ApiScope>();

            foreach (var scope in scopeName.Split(" "))
            {
                newApiScopes.Add(new ApiScope(scope, scope));
            }

            _configurationDbContext.ApiScopes.AddRange(newApiScopes.Select(x => x.ToEntity()));
        }
    }
}
