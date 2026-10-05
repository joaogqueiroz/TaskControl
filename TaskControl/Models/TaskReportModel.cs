using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskControl.Models
{
    public class TaskReportModel : IValidatableObject
    {
        [Required(ErrorMessage = "Please inform the start date")]
        [ValidDate(ErrorMessage = "Please inform a valid start date")]
        public string StartDate { get; set; }

        [Required(ErrorMessage = "Please inform the finish date")]
        [ValidDate(ErrorMessage = "Please inform a valid finish date")]
        public string FinishDate { get; set; }

        // Runs after the attributes pass, so both dates are valid yyyy-MM-dd here
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.CompareOrdinal(FinishDate, StartDate) < 0)
            {
                yield return new ValidationResult("The finish date cannot be before the start date", new[] { nameof(FinishDate) });
            }
        }
    }
}
