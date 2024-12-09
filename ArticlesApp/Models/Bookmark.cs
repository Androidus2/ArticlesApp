using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArticlesApp.Models
{
    public class Bookmark
    {

        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Numele colectiei este obligatoriu")]
        public string? Name { get; set; }
        [Required]
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<ArticleBookmark>? ArticleBookmarks { get; set; }

    }
}
