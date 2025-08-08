namespace SchoolProjectWeb.Models
{
    public class AssignStudentToClassroomViewModel
    {
        public List<User> Students { get; set; }
        public List<ClassroomViewModel> Classrooms { get; set; }
        public int SelectedUserId { get; set; }
        public int SelectedClassroomId { get; set; }
    }
}