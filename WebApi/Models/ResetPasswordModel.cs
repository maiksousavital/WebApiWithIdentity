using System.ComponentModel.DataAnnotations;


namespace WebApi.Models
{
    public class ResetPasswordModel : IValidatableObject
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]         
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password != ConfirmPassword)
            {
                yield return new ValidationResult("The password and confirmation password do not match.");
            }
        }
    }
}
