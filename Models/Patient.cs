using System;
using System.ComponentModel.DataAnnotations;

namespace test2.Models
{
    public class Patient
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        [Key]
        public Guid PatientId { get; set; }
    }
}
