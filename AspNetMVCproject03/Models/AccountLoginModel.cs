using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetMVCproject03.Models
{
    public class AccountLoginModel
    {
        [EmailAddress(ErrorMessage ="Please enter with a valid email.")]
        [Required(ErrorMessage = "Please type your email.")]
        public string Email { get; set; }

        
        [MaxLength(20, ErrorMessage = "Password should have {1} maximum characters.")]
        [MinLength(8, ErrorMessage = "Password should have {1} minimum characters.")]
        [Required(ErrorMessage = "Please type your password.")]
        public string PassWord { get; set; }
    }
}
