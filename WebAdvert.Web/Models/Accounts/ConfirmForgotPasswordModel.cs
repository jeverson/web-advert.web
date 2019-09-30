using System.ComponentModel.DataAnnotations;

namespace WebAdvert.Web.Models.Accounts
{
    public class ConfirmForgotPasswordModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }


        [Required(ErrorMessage = "Code is required.")]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(6, ErrorMessage = "Password must be at least six characteres long!")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and its confirmation do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
