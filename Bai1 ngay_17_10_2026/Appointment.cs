using System;

namespace Bai1_ngay_17_10_2026
{
    public class Appointment
    {
        public string Id { get; set; }
        public string PatientName { get; set; }
        public string Phone { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string Doctor { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }

        public Appointment()
        {
            Id = string.Empty;
            PatientName = string.Empty;
            Phone = string.Empty;
            AppointmentTime = DateTime.Now;
            Doctor = string.Empty;
            Reason = string.Empty;
            Status = string.Empty;
        }
    }
}
