using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace Appointments.Database.Dto
{
    public class DoctorsDto
    {
        public int DoctorKey { get; set; }
        public string Last { get; set; } = string.Empty;
        public string First { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public List<LocationDto> Locations { get; set; } = new List<LocationDto>();
    }
}
