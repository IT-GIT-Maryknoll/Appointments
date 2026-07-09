using Microsoft.AspNetCore.Routing;
using System;
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
        public string CreatedBy { get; set; } = string.Empty;
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime AddDate { get; set; }
        public DateTime ModDate { get; set; }
        public List<LocationDto> Locations { get; set; } = new List<LocationDto>();
        public bool IsDeleted { get; set; } = false;
        public DoctorsDto Clone()
        {
            return this.MemberwiseClone() as DoctorsDto;
        }
    }
}
