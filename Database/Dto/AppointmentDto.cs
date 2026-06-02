using System;
using System.Collections.Generic;

namespace Appointments.Database.Dto
{
    public class AppointmentDto
    {
        public int ID { get; set; }
        public DateTime? ApptTime { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public int DoctorKey { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;   
        public int LocationID { get; set; }
        public int DriverKey { get; set; }
        public string DriverName { get; set; }  = string.Empty;
        public string CarNum { get; set; } = string.Empty;
        public string CarInfo { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty; 
        public string Notes { get; set; } = string.Empty;
        public DateTime? Depart { get; set; }
        public int ResidentKey { get; set; }
        public bool MakeAppointment { get; set; }
        public bool ConfirmedAppointment { get; set; }
        public string NurseName { get; set; } = string.Empty; 
        public string PhoneNumber { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime AddDate { get; set; }
        public DateTime ModDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int PrepID { get; set; }
        public bool InHouseVisit { get; set; }
        public bool NursesAideAccompaniment { get; set; }
        public string ApptType { get; set; } = string.Empty;
        public List<LocationDto> Locations { get; set; } = new List<LocationDto>();
        public string Car_Model { get; set; } = string.Empty;
    }
}
