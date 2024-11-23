﻿using System.ComponentModel.DataAnnotations;

namespace ArticlesApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul comentariului nu poate fi gol!")]
        public string? Content { get; set; }

        public DateTime Date { get; set; }

        public int? ArticleId { get; set; }

        public virtual Article? Article { get; set; }
    }
}