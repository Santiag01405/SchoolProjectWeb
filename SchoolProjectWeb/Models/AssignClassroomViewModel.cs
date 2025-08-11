// Models/AssignClassroomViewModel.cs

using System.ComponentModel.DataAnnotations;

namespace SchoolProjectWeb.Models
{
    public class AssignClassroomViewModel
    {
        [Required]
        public int CourseID { get; set; }
        [Required]
        public int ClassroomID { get; set; }
    }
}