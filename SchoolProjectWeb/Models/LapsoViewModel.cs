using System.ComponentModel.DataAnnotations;
using System;

namespace SchoolProjectWeb.Models
{
    public class Lapso
    {
        public int LapsoID { get; set; }

        [Required(ErrorMessage = "El nombre del lapso es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Fin")]
        public DateTime FechaFin { get; set; }

        public int SchoolID { get; set; }
    }
}