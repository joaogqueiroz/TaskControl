using AspNetMVCproject03.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspNetMVCproject03.Reports.Data
{
    public class TaskReportData
    {
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public User User { get; set; }
        public List<Task> Tasks { get; set; }
    }
}
