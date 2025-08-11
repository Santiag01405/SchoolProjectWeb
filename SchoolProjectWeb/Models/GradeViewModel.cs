namespace SchoolProjectWeb.Models
{
    public class GradeViewModel
    {
        public int EvaluationID { get; set; }
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public decimal? GradeValue { get; set; }
        public string? Comments { get; set; }
        public bool HasGrade { get; set; }
    }
}