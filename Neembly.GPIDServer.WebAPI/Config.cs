using IdentityServer4;
using IdentityServer4.Models;
using Neembly.GPIDServer.Constants;
using Neembly.GPIDServer.WebAPI.Models.Configs;
using System;
using System.Collections.Generic;

namespace Neembly.GPIDServer.WebAPI
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApiResources(List<AuthClientResources> authResources)
        {
            List<ApiResource> result = new List<ApiResource>();
            if (authResources != null)
            { 
                foreach (var authResourceItem in authResources)
                {
                    result.Add(new ApiResource(authResourceItem.Name)
                                {
                                    ApiSecrets =
                                    {
                                        new Secret(authResourceItem.SecretKey.Sha256())
                                    },
                                    Scopes =
                                    {
                                        new Scope
                                        {
                                            Name = authResourceItem.Request,
                                            DisplayName = authResourceItem.Name
                                        }
                                    },
                                    UserClaims = { "role", "user", $"{authResourceItem.Name}"}
                    });
                }
            }
            return result;
         }

        public static IEnumerable<Client> GetClients(List<AuthClientInfo> authClients)
        {
            List<Client> result = new List<Client>();
            if (authClients != null)
            {
                foreach (var authClientItem in authClients)
                {
                    if (authClientItem.Type.Equals(GlobalConstants.AuthTypePassword, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.Add(
                            new Client
                            {
                                ClientId = authClientItem.ClientId,
                                AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                                AccessTokenType = AccessTokenType.Jwt,
                                AccessTokenLifetime = authClientItem.LifeTime,
                                IdentityTokenLifetime = authClientItem.LifeTime,
                                UpdateAccessTokenClaimsOnRefresh = true,
                                SlidingRefreshTokenLifetime = 30,
                                AllowOfflineAccess = true,
                                RefreshTokenExpiration = TokenExpiration.Absolute,
                                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                                AlwaysSendClientClaims = true,
                                ClientSecrets = new List<Secret> { new Secret(authClientItem.SecretKey.Sha256()) },
                                AllowedScopes = {
                                    IdentityServerConstants.StandardScopes.OpenId,
                                    IdentityServerConstants.StandardScopes.Profile,
                                    IdentityServerConstants.StandardScopes.Email,
                                    IdentityServerConstants.StandardScopes.OfflineAccess,
                                    authClientItem.ApiScope
                                }
                            }
                        );
                    }
                    if (authClientItem.Type.Equals(GlobalConstants.AuthTypeClientCredentials, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.Add(
                            new Client
                            {
                                ClientId = authClientItem.ClientId,
                                AllowedGrantTypes = GrantTypes.ClientCredentials,
                                ClientSecrets = new List<Secret> { new Secret(authClientItem.SecretKey.Sha256()) },
                                AllowedScopes = {authClientItem.ApiScope}
                            }
                        );
                    }
                }
            }
            return result;
        }
    }
}
