using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskControl.Models
{
    public class TaskRegistrationModel
    {
        [MaxLength(150, ErrorMessage = "Task name should have {1} maximum characters.")]
        [Required(ErrorMessage = "Inform task name.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Select task date.")]
        [ValidDate(ErrorMessage = "Select a valid task date.")]
        public string Date { get; set; }

        [Required(ErrorMessage = "Inform task hour.")]
        [ValidTime(ErrorMessage = "Inform a valid task hour.")]
        public string Hour { get; set; }

        [MaxLength(100, ErrorMessage = "Description should have {1} maximum characters.")]
        [Required(ErrorMessage = "Inform task description.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Select task priority.")]
        public TaskPriority? Priority { get; set; }
    }
    public enum TaskPriority 
    {
        HIGH,
        MEDIUM,
        LOW
    }
}
