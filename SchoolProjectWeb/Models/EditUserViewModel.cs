namespace SchoolProjectWeb.Models
{
    public class EditUserViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // solo para entrada
        public int RoleID { get; set; }

        public string PasswordHash { get; set; } = string.Empty;
    }
}
