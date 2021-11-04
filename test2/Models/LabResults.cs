using System;
using System.ComponentModel.DataAnnotations;

namespace test2.Models
{
    
    public class LabResults
    {
        [Required(ErrorMessage = "LabType is required")]
        public string LabType { get; set; }
        public string Result { get; set; }
        public string TestTime { get; set; }
        [Required(ErrorMessage = "EnteredTime is required")]
        public string EnteredTime { get; set; }
        [Required(ErrorMessage = "PatientID is required")]
        public Guid PatientID { get; set; }
        [Key]
        public Guid LabID { get; set; }
    }
}
