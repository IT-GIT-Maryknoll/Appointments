namespace Appointments.Database.Dto
{
    public class LocationDto
    {
        public int DoctorKey { get; set; }
        public int LocationID { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public bool FMC { get; set; } = false;
        public bool AdmittingFacility { get; set; } = false;
        public bool InHouse { get; set; } = false;
    }
}
