using IdentityServer4.Validation;
using Neembly.GPIDServer.Persistence;
using System.Linq;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.WebAPI.Validator
{
    public class CustomResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly AppDBContext _context;

        public CustomResourceOwnerPasswordValidator(AppDBContext context)
        {
            _context = context;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {


            var user = _context.Users.Where(p => p.UserName == context.UserName).FirstOrDefault();
                context.Result = new GrantValidationResult(user.UserName, OidcConstants.AuthenticationMethods.Password);


            return Task.FromResult(0);
        }
    }
}
