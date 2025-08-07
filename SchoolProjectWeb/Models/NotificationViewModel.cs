namespace SchoolProjectWeb.Models
{
    public class NotificationSendViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Target { get; set; } = "all"; // all, role, user
        public int? RoleID { get; set; }
        public int? UserID { get; set; }
    }
}
