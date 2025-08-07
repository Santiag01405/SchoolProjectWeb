namespace SchoolProjectWeb.Models
{
    public class UserRegisterViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public List<RoleOption> Roles { get; set; } = new()
        {
            new RoleOption { Name = "Estudiante", Id = 1 },
            new RoleOption { Name = "Profesor", Id = 2 },
            new RoleOption { Name = "Padre", Id = 3 }
        };
    }

    public class RoleOption
    {
        public string Name { get; set; } = "";
        public int Id { get; set; }
    }
}
