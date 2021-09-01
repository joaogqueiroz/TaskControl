using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AspNetMVCproject03.Models
{
    public class TaskEditModel
    {
        public Guid TaskID { get; set; }

        [Required(ErrorMessage = "Inform task name.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Select task date.")]
        public string Date { get; set; }

        [Required(ErrorMessage = "Inform task hour.")]
        public string Hour { get; set; }

        [MaxLength(150, ErrorMessage = "Description should have {1} maximum characters.")]
        [Required(ErrorMessage = "Inform task description.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Select task priority.")]
        public TaskPriority? Priority { get; set; }
    }

}

