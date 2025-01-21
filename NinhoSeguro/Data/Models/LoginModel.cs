using System.ComponentModel.DataAnnotations;

namespace LI4.Data.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "O username é obrigatório.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "A password é obrigatória.")]
        public string Password { get; set; }
    }
}