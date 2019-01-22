using IdentityServer4;
using IdentityServer4.Models;
using Neembly.GPIDServer.Constants;
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


        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api1", "My API")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            // client credentials client
            return new List<Client>
            {
                
                // resource owner password grant client
                new Client
                {
                    ClientId = GlobalConstants.IdServerClient,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret(GlobalConstants.IdServerSecret.Sha256())
                    },
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Address,
                        GlobalConstants.IdServerApiScope
                    }
                },

                new Client
                {
                    ClientId = GlobalConstants.IdServerClientToken,
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret(GlobalConstants.IdServerSecret.Sha256())
                    },
                    AllowedScopes = { GlobalConstants.IdServerApiScope }
                }


            };
        }
               
    }
}
