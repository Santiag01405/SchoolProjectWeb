namespace SchoolProjectWeb.Models
{
    public class UserRelationshipViewModel
    {
        public int? RelationID { get; set; }  // Ej: 1 (Padre-Hijo)
        public int User1ID { get; set; }  // Ej: Padre
        public int User2ID { get; set; }  // Ej: Hijo
        public string RelationshipType { get; set; } = "Padre-Hijo";

        public List<User>? Parents { get; set; }
        public List<User>? Children { get; set; }
    }

}
