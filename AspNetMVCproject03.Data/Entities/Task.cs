using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetMVCproject03.Data.Entities
{
    public class Task
    {
        public Guid TaskID { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Hour { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public Guid UserID { get; set; }
        //Have one
        public User User { get; set; }
    }
}
