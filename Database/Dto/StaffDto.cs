namespace Appointments.Database.Dto
{
    public class StaffDto
    {
        public string EmployeeID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
