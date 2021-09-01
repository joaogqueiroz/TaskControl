using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetMVCproject03.Data.Entities
{
    public class User
    {
        public Guid UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PassWord { get; set; }
        public DateTime RegistrationDate { get; set; }

        //Have many
        public List<Task> Tasks { get; set; }
    }
}
