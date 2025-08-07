using System.ComponentModel.DataAnnotations;

namespace SchoolProjectWeb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico o nombre de usuario es obligatorio.")]
        public string EmailOrUserName { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}