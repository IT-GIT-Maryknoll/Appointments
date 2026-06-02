namespace Appointments.Database.Dto
{
    public class UserDto
    {
        public string sAMAccountName { get; set; }
        public string cn { get; set; }
        public string FirstName { get; set; }
        public string MI { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string MbrEmpIndicator { get; set; }
        public string MbrEmpNumber { get; set; }
        public string EmailAddress { get; set; }
        public string ADSDescription { get; set; }
        public bool msExchHideFromAddressLists { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsRegional { get; set; }
        public string Regions { get; set; } = "";
    }
}
