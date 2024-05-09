using System.ComponentModel.DataAnnotations;

namespace Domain.Bislerium.ViewModels
{
    public class ChangePasswordViewModel
    {
        public string PreviousPassword { get; set; }
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
