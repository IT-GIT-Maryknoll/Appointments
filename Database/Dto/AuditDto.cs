using System;

namespace Appointments.Database.Dto
{
    public class AuditDto
    {
        public int Id { get; set; } 
        public DateTime Datetime { get; set; }
        public string Ip { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Table { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
