namespace Neembly.GPIDServer.SharedClasses
{
    public class SSOPlayerRegister
    {
        public int PlayerId { get; set; }
        public int OperatorId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string CountryCode { get; set; }
        public string CreatedBy { get; set; }
        public bool BoUser { get; set; }
        public string SSOAuthProvider { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string MobileNumber { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string Currency { get; set; }
        public string BirthDate { get; set; }
        public string Gender { get; set; }
        public string PromoCode { get; set; }
        public string RegistrationIPAddress { get; set; }
    }
}
