using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetMVCproject03.Models
{
    public class AccountPasswordRecoveryModel
    {
        [EmailAddress(ErrorMessage = "Inform a valid email.")]
        [Required(ErrorMessage = "Inform your email.")]
        public string Email { get; set; }
    }
}
