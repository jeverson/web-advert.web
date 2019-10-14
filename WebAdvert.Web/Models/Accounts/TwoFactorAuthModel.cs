using System.ComponentModel.DataAnnotations;

namespace WebAdvert.Web.Models.Accounts
{
    public class TwoFactorAuthModel
    {
        [Required( ErrorMessage =  "Please inform your email.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please inform the confirmation code.")]
        public string Code { get; set; }
    }
}
