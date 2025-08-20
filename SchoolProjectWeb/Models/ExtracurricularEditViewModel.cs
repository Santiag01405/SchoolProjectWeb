using System.ComponentModel.DataAnnotations;

namespace SchoolProjectWeb.Models
{
    public class ExtracurricularEditViewModel
    {
        public int ActivityID { get; set; }
        public int SchoolID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un día de la semana.")]
        public int? DayOfWeek { get; set; }

        public string UserID { get; set; }

        public List<User> Teachers { get; set; }
    }
}