using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArticlesApp.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul nu poate fi gol!")]
        [StringLength(100, ErrorMessage = "Titlul trebuie sa contina maxim 100 de caractere!")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa contina minim 5 caractere!")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Continutul articolului nu poate fi gol!")]
        public string? Content { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Categoria nu poate fi goala!")]
        public int CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        [NotMapped]
		public IEnumerable<SelectListItem>? Categ { get; set; }
	}
}
