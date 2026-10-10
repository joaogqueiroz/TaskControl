using System.ComponentModel.DataAnnotations;

namespace TaskControl.Models
{
    public class AccountResetPasswordModel
    {
        [Required(ErrorMessage = "This link is invalid. Ask for a new one.")]
        public string Token { get; set; }
        [StrongPassWord(ErrorMessage = "Inform a uppercase letter 1, 1 lowercase letter, 1 number and 1 special character(! @ # $ % & ).")]
        [MaxLength(20, ErrorMessage = "Type a maximum of {1} characters")]
        [MinLength(8, ErrorMessage = "Type a minimum of {1} characters")]
        [Required(ErrorMessage = "Inform your new password.")]
        public string NewPassWord { get; set; }
        [Compare("NewPassWord", ErrorMessage = "The passwords don't are equals")]
        [Required(ErrorMessage = "Confirm your new password")]
        public string NewPassWordConfirm { get; set; }
    }
}
