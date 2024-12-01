using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Controllers
{
    public class CommentsController : Controller
    {

        private readonly ApplicationDbContext db;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly RoleManager<IdentityRole> roleManager;

        public CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            db = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpGet]
		[Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Delete(int id)
		{
			try
			{
				Comment comment = db.Comments.Find(id);
				int? articleId = comment.ArticleId;

				if(comment.UserId != userManager.GetUserId(User) && !User.IsInRole("Admin"))
                {
                    TempData["error"] = "Nu aveti dreptul sa stergeti acest comentariu!";
                    return RedirectToAction("Show", "Articles", new { id = articleId });
                }


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
