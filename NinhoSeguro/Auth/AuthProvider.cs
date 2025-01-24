using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace LI4.Auth
{
    public class AuthProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _storage;
        private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

        public AuthProvider(ProtectedSessionStorage storage)
        {
            _storage = storage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var sessionResult = await _storage.GetAsync<Session>("UserSession");
                var session = sessionResult.Success ? sessionResult.Value : null;

                if (session == null)
                {
                    return await Task.FromResult(new AuthenticationState(_anonymous));
                }

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, session.Id.ToString()), 
                    new(ClaimTypes.Name, session.Nome),
                    new(ClaimTypes.Role, session.Tipo == 1 ? "Cliente" : "Funcionário")
                }, "CustomAuth"));

                return await Task.FromResult(new AuthenticationState(claimsPrincipal));
            }
            catch
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }
        }

        public async Task UpdateAuthenticationState(Session? session)
        {
            ClaimsPrincipal claimsPrincipal;

            if (session != null)
            {
                await _storage.SetAsync("UserSession", session);

                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, session.Id.ToString()) ,
                    new(ClaimTypes.Name, session.Nome),
                    new(ClaimTypes.Role, session.Tipo == 1 ? "Cliente" : "Funcionário")
                }, "CustomAuth"));
            }
            else
            {
                await _storage.DeleteAsync("UserSession");
                claimsPrincipal = _anonymous;
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task Logout()
        {
            await UpdateAuthenticationState(null);
        }
    }
}
