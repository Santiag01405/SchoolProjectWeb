namespace SchoolProjectWeb.Models
{
    public class EnrollmentViewModel
    {
        public int EnrollmentID { get; set; }
        public int UserID { get; set; }
        public int CourseID { get; set; }

        public string? UserName { get; set; }
        public string? CourseName { get; set; }

        public List<User>? Users { get; set; }
        public List<Course>? Courses { get; set; }
    }

}
