using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Controllers
{
    public class CommentsController : Controller
    {

        private readonly ApplicationDbContext db;

		public CommentsController(ApplicationDbContext db)
		{
			this.db = db;
		}

		[HttpGet]
		public IActionResult Delete(int id)
		{
			try
			{
				Comment comment = db.Comments.Find(id);
				int? articleId = comment.ArticleId;
				db.Comments.Remove(comment);
				db.SaveChanges();
                TempData["message"] = "Comentariul a fost sters!";
                return RedirectToAction("Show", "Articles", new { id = articleId });
			}
			catch (Exception)
			{
                TempData["error"] = "Comentariul nu a putut fi sters!";
                return RedirectToAction("Index", "Articles");
			}
		}
	}
}
