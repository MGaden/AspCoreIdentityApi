using System.ComponentModel.DataAnnotations;

namespace CoreIdentity.API.Identity.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string ClientCallbackUrl { get; set; }
    }
}
