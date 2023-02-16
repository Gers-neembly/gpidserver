using System.Security.Claims;
using System.Security.Principal;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;

namespace Neembly.GPIDServer.SharedServices.SSO
{
    public class SSOClaimsService : ISSOClaimsService
    {
        const string DefaultPlayerName = "PLYR";
        public SSOClaimsService()
        {
        }

        public SSOUserInfo GetSSOUserInfo(IPrincipal user)
        {
            return new SSOUserInfo
            {
                FullName = GetFullName(user),
                Email = GetEmail(user),
                FirstName = GetFirstName(user),
                LastName = GetLastName(user),
                BirthDate = GetBirthDate(user),
                Gender = GetGender(user),
                HomePhone = GetHomePhone(user),
                MobilePhone = GetMobilePhone(user),
                Address = GetAddress(user),
                State = GetStateOrProvince(user),
                PostalCode = GetPostalCode(user),
                Country = GetCountry(user),
                Locale = GetLocale(user),
                Sid = GetSid(user),
                PrimarySid = GetPrimarySid(user),
                NameIdentifier = GetNameId(user)
            };
        }

        public string GenerateUsername(IPrincipal user)
        {
            string firstName = GetFirstName(user);
            string playerTag = GenerateUserIdTag();
            if (!string.IsNullOrEmpty(firstName))
                return $"{firstName}{playerTag}";
            else
                return $"{DefaultPlayerName}{playerTag}";
        }

        // ustils
        #region
        private string GetFullName(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Name);
            return claim == null ? null : claim.Value;
        }
        private string GetEmail(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Email);
            return claim == null ? null : claim.Value;
        }
        private string GetFirstName(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.GivenName);
            return claim == null ? null : claim.Value;
        }
        private string GetLastName(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Surname);
            return claim == null ? null : claim.Value;
        }
        private string GetBirthDate(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.DateOfBirth);
            return claim == null ? null : claim.Value;
        }
        private string GetGender(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Gender);
            return claim == null ? null : claim.Value;
        }
        private string GetHomePhone(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.HomePhone);
            return claim == null ? null : claim.Value;
        }
        private string GetMobilePhone(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.MobilePhone);
            return claim == null ? null : claim.Value;
        }
        private string GetAddress(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.StreetAddress);
            return claim == null ? null : claim.Value;
        }
        private string GetStateOrProvince(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.StateOrProvince);
            return claim == null ? null : claim.Value;
        }
        private string GetPostalCode(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.PostalCode);
            return claim == null ? null : claim.Value;
        }
        private string GetCountry(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Country);
            return claim == null ? null : claim.Value;
        }
        private string GetLocale(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Locality);
            return claim == null ? null : claim.Value;
        }
        private string GetSid(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Sid);
            return claim == null ? null : claim.Value;
        }
        private string GetPrimarySid(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.PrimarySid);
            return claim == null ? null : claim.Value;
        }
        private string GetNameId(IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.NameIdentifier);
            return claim == null ? null : claim.Value;
        }

        private string GenerateUserIdTag()
        {
            return (new System.Random()).Next(999, 99999).ToString("D5");
        }
        #endregion
    }
}
