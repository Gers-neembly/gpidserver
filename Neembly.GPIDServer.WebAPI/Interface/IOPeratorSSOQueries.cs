using Neembly.GPIDServer.WebAPI.Models.Configs;
using System.Collections.Generic;

namespace Neembly.GPIDServer.WebAPI.Interface
{
    public interface IOperatorSSOQueries
    {
        List<GoogleAuthProviderData> GetperatorSSOByProvider(string authProvider);
    }
}
