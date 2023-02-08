using Neembly.GPIDServer.SharedClasses;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace Neembly.GPIDServer.SharedServices.SSO
{
    public static class UserClaimsExtend
    {
        public static string GetFullName(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Name);
            return claim == null ? null : claim.Value;
        }
        public static string GetEmail(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Email);
            return claim == null ? null : claim.Value;
        }
        public static string GetFirstName(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.GivenName);
            return claim == null ? null : claim.Value;
        }
        public static string GetLastName(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Surname);
            return claim == null ? null : claim.Value;
        }
        public static string GetBirthDate(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.DateOfBirth);
            return claim == null ? null : claim.Value;
        }
        public static string GetGender(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Gender);
            return claim == null ? null : claim.Value;
        }
        public static string GetHomePhone(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.HomePhone);
            return claim == null ? null : claim.Value;
        }
        public static string GetMobilePhone(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.MobilePhone);
            return claim == null ? null : claim.Value;
        }
        public static string GetAddress(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.StreetAddress);
            return claim == null ? null : claim.Value;
        }
        public static string GetStateOrProvince(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.StateOrProvince);
            return claim == null ? null : claim.Value;
        }
        public static string GetPostalCode(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.PostalCode);
            return claim == null ? null : claim.Value;
        }
        public static string GetCountry(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Country);
            return claim == null ? null : claim.Value;
        }
        public static string GetLocale(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst(ClaimTypes.Locality);
            return claim == null ? null : claim.Value;
        }

        public static SSOUserInfo GetSSOUserInfo(IPrincipal user)
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
                Locale = GetLocale(user)
            };
        }
    }
}
