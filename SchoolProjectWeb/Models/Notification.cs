using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SchoolProjectWeb.Models
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        [Column("notificationID")]
        public int NotifyID { get; set; }

        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Content { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Column("isRead")]
        public bool IsRead { get; set; } = false;

        [ForeignKey("User")]
        public int UserID { get; set; }
        public User? User { get; set; }
    }
}
