using Microsoft.AspNetCore.Identity;

namespace ArticlesApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Un utilizator poate avea mai multe articole
        public virtual ICollection<Article>? Articles { get; set; }

        // Un utilizator poate avea mai multe comentarii
        public virtual ICollection<Comment>? Comments { get; set; }
    }
}
