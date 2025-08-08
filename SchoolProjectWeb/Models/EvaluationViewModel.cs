using System.ComponentModel.DataAnnotations;

namespace SchoolProjectWeb.Models
{
    public class EvaluationViewModel
    {
        [Required(ErrorMessage = "El título es obligatorio.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un curso.")]
        public int CourseID { get; set; }
    }
}