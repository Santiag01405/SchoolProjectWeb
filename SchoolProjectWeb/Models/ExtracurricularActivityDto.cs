namespace SchoolProjectWeb.Models
{
    public class ExtracurricularActivityDto
    {
        public int ActivityID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? UserID { get; set; }
        public object User { get; set; }
        public int DayOfWeek { get; set; }
        public int SchoolID { get; set; }
        public object School { get; set; }
        public List<object> ExtracurricularEnrollments { get; set; }

    }
}
