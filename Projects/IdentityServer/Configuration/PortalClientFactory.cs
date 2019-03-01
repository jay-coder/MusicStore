using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JayCoder.MusicStore.Projects.IdentityServer.Configuration
{
    public class PortalClientFactory
    {
        private IConfiguration _configuration;
        private IEnumerable<IConfigurationSection> _clientList;
        public PortalClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
            try
            {
                var clients = _configuration.GetSection("Clients");
                _clientList = clients.GetChildren();
            }
            catch
            {
                throw new MissingFieldException("IConfiguration missing Clients section");
            }
        }
        public IEnumerable<Client> GetClients()
        {
            if (_clientList != null && _clientList.Any())
            {
                foreach (var client in _clientList)
                {
                    yield return new Client()
                    {
                        ClientId = client.GetValue<string>("ClientId"),
                        ClientName = client.GetValue<string>("ClientName"),
                        ClientUri = client.GetValue<string>("ClientUri"),
                        RequireConsent = client.GetValue<bool>("RequireConsent"),
                        AllowedGrantTypes = client.GetValue<string>("AllowedGrantTypes")
                            .Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries),
                        AllowAccessTokensViaBrowser = client.GetValue<bool>("AllowAccessTokensViaBrowser"),
                        RedirectUris = client.GetValue<string>("RedirectUris")
                            .Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries),
                        PostLogoutRedirectUris = client.GetValue<string>("PostLogoutRedirectUris")
                            .Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries),
                        AllowedCorsOrigins = client.GetValue<string>("AllowedCorsOrigins")
                            .Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries),
                        AllowedScopes = client.GetValue<string>("AllowedScopes")
                            .Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries),
                        AlwaysIncludeUserClaimsInIdToken = client.GetValue<bool>("AlwaysIncludeUserClaimsInIdToken"),
                        IdentityTokenLifetime = client.GetValue<int>("IdentityTokenLifetime"),
                        AccessTokenLifetime = client.GetValue<int>("AccessTokenLifetime")
                    };
                }
            }
        }
    }
}
