using DyntellProject.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace DyntellProject.Core.Entities
{
    public class User : IdentityUser
    {
        public AgeGroup AgeGroup { get; set; }
        public UserRole Role { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
