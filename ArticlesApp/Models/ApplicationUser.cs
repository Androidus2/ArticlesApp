using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArticlesApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Un utilizator poate avea mai multe articole
        public virtual ICollection<Article>? Articles { get; set; }

        // Un utilizator poate avea mai multe comentarii
        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<Bookmark>? Bookmarks { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
