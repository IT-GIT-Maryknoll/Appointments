namespace Appointments.Database.Dto
{
    public class ResidentsDto
    {
        public int ResidentKey { get; set; }
        public string FullName { get; set; } = "";
        public int ID { get; set; }
        public string Name { get; set; } = "";
        public string Ext { get; set; } = "";
        public string Room { get; set; } = "";
        public string Country { get; set; } = "";
        public string DOB { get; set; } = "";
        public bool OnStaff { get; set; } = false;
        public string DateAssigned { get; set; } = "";
        public string Perm { get; set; } = "";
        public string ResidentNotes { get; set; } = "";
        public string Location { get; set; } = "";
        public string Status { get; set; } = "";
        public string Res_Title { get; set; } = "";
        public string Res_FirstName { get; set; } = "";
        public string Res_LastName { get; set; } = "";
        public string Res_MiddleInt { get; set; } = "";
        public ResidentsDto Clone()
        {
            return this.MemberwiseClone() as ResidentsDto;
        }
    }
    public class StatusDto
    {
        public int ID { get; set; }
        public string StatusName { get; set; } = "";
    }
}
