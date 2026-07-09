namespace Appointments.Database.Dto
{
    public class CarDto
    {
        public int CarNum { get; set; }
        public string CarMake { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string CarInfo { get; set; }= string.Empty;
        public string CarName { get; set; } = string.Empty;
        public bool InActive { get; set; } = false;
    }
}
