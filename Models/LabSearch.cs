using System.ComponentModel.DataAnnotations;

namespace test2.Models
{
    public class LabSearch
    {
        [Required(ErrorMessage = "LabType is required")]
        public string LabType { get; set; }
        [Required(ErrorMessage = "FromDate is required")]
        public string FromDate { get; set; }
        [Required(ErrorMessage = "ToDate is required")]
        public string ToDate { get; set; }
    }
}
