// EN: Models/ExtracurricularViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace SchoolProjectWeb.Models
{
    public class ExtracurricularViewModel
    {
        public int ActivityID { get; set; }

        [Required(ErrorMessage = "El nombre de la actividad es obligatorio.")]
        [Display(Name = "Nombre de la Actividad")]
        public string Name { get; set; }

        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Display(Name = "Día de la Semana")]
        public int? DayOfWeek { get; set; }

        [Display(Name = "Profesor a Cargo")]
        public int? UserID { get; set; } // ID del profesor

        public int SchoolID { get; set; }

        // Esta propiedad la usaremos para mostrar la lista de profesores
        // en el formulario de creación/edición.
        public IEnumerable<User>? Teachers { get; set; }
    }
}