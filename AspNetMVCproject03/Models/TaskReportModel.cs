using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetMVCproject03.Models
{
    public class TaskReportModel
    {
        [Required(ErrorMessage = "Please inform the start date")]
        public string StartDate { get; set; }

        [Required(ErrorMessage = "Please inform the finish date")]
        public string FinishDate { get; set; }
    }
}
