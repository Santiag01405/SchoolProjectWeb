namespace SchoolProjectWeb.Models
{
    public class Course
    {
        public int CourseID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DayOfWeek { get; set; }
        public int UserID { get; set; } // Profesor
    }
}
