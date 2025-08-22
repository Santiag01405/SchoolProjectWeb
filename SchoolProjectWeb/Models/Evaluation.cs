namespace SchoolProjectWeb.Models
{
    public class Evaluation
    {
        public int EvaluationID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public int CourseID { get; set; }
        public int UserID { get; set; }
        public int SchoolID { get; set; }
        public int? ClassroomID { get; set; }
        public int LapsoID { get; set; }
        public Lapso Lapso { get; set; }
    }
}