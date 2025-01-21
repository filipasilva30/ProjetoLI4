using System.ComponentModel.DataAnnotations;

namespace LI4.Data.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "O username é obrigatório.")]
        [RegularExpression(@"^cl.+$", ErrorMessage = "O username deve começar com 'cl'.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A password é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A password deve ter entre 6 e 100 caracteres.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "O contacto telefónico é obrigatório.")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "O contato telefónico deve ter 9 dígitos.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "O NIF é obrigatório.")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "O NIF deve ter 9 dígitos.")]
        public string NIF { get; set; }
    }
}
