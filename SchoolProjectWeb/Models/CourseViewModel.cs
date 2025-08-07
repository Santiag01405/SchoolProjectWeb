namespace SchoolProjectWeb.Models
{
    public class CourseViewModel
    {
        public int CourseID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public string DayOfWeekName
        {
            get
            {
                return DayOfWeek switch
                {
                    1 => "Lunes",
                    2 => "Martes",
                    3 => "Miércoles",
                    4 => "Jueves",
                    5 => "Viernes",
                    6 => "Sábado",
                    0 => "Domingo",
                    _ => "Desconocido"
                };
            }
        }
        public int UserID { get; set; } // ID del profesor

        public List<User>? Teachers { get; set; } // Lista de profesores para dropdown
        public string? TeacherName { get; set; }
    }
}
