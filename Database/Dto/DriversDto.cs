namespace Appointments.Database.Dto
{
    public class DriversDto
    {
        public int DriverID { get; set; }
        public string DriverName { get; set; }
        public string Last { get; set; }
        public string First { get; set; }
        public string Title { get; set; }
        public int Priority { get; set; }
        public bool InActive { get; set; }
    }
}
