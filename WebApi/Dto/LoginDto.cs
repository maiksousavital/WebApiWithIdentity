using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace WebApi.Dto
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty.ToString();
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
