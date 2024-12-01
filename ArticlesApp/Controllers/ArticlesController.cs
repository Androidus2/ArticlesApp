using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Controllers
{
	public class ArticlesController : Controller
	{
		private readonly ApplicationDbContext db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

        public ArticlesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IEnumerable<SelectListItem> GetAllCategories()
		{
			var selectList = new List<SelectListItem>();
			var categories = db.Categories.ToList();
			foreach (var category in categories)
			{
				selectList.Add(new SelectListItem
				{
					Value = category.Id.ToString(),
					Text = category.CategoryName
				});
			}
			return selectList;
		}

		[Authorize(Roles = "User,Editor,Admin")]
		public IActionResult Index()
		{
            var articles = db.Articles
                            .Include("Category")
                            .Include("User")
                            .ToList();
            ViewBag.Articles = articles;

			return View();
		}


		[HttpGet]
		[Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
		{
			var article = db.Articles
							.Include("Category")
							.Include("Comments")
							.Include("User")
							.Include("Comments.User")
                            .FirstOrDefault(art => art.Id == id);

            if (article == null)
			{
                TempData["error"] = "Articolul nu exista!";
                return RedirectToAction("Index");
            }

			ViewBag.CurrentUserId = _userManager.GetUserId(User);
            SetAccessRights(article.UserId);
			
			return View(article);
		}


		[HttpGet]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New()
		{
			try
			{
				Article article = new Article();
				article.Categ = GetAllCategories();
				return View(article);
			}
			catch (Exception)
			{
				return RedirectToAction("Index");
			}
		}

		[HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New(Article article)
		{
            article.Date = DateTime.Now;

			article.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
			{
                db.Articles.Add(article);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost adaugat!";
                return RedirectToAction("Index");
            }
            article.Categ = GetAllCategories();
			TempData["error"] = "Datele introduse sunt incorecte!";
            return View(article);
        }

		[HttpGet]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id)
		{
			try
			{
				var article = db.Articles.Find(id);

				if(article.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                {
                    TempData["error"] = "Nu aveti dreptul sa editati acest articol!";
                    return RedirectToAction("Index");
                }

                article.Categ = GetAllCategories();
				return View(article);
			}
			catch (Exception)
			{
				return RedirectToAction("Index");
			}
		}

		[HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id, Article requestArticle)
		{

            if (ModelState.IsValid)
            {
                var article = db.Articles.Find(id);

                if (article.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                {
                    TempData["error"] = "Nu aveti dreptul sa editati acest articol!";
                    return RedirectToAction("Index");
                }

                article.Title = requestArticle.Title;
                article.Content = requestArticle.Content;
                article.CategoryId = requestArticle.CategoryId;
                article.Date = DateTime.Now;
                db.SaveChanges();
                TempData["message"] = "Articolul a fost modificat!";
                return RedirectToAction("Show", new { id = article.Id });
            }
            requestArticle.Categ = GetAllCategories();
            TempData["error"] = "Datele introduse sunt incorecte!";
            return View(requestArticle);
        }

		[HttpGet]
		[Authorize(Roles = "Editor,Admin")]
		public IActionResult Delete(int id)
		{
			try
			{
				var article = db.Articles.Find(id);
				if (article != null)
				{

					if(article.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                    {
                        TempData["error"] = "Nu aveti dreptul sa stergeti acest articol!";
                        return RedirectToAction("Index");
                    }

                    var comments = from com in db.Comments
								   where com.ArticleId == article.Id
								   select com;
					foreach (var comment in comments)
					{
						db.Comments.Remove(comment);
					}

					db.Articles.Remove(article);
					db.SaveChanges();
				}
				TempData["message"] = "Articolul a fost sters!";
                return RedirectToAction("Index");
			}
			catch (Exception)
			{
                TempData["error"] = "Eroare la stergerea articolului!";
                return RedirectToAction("Index");
			}
		}


		[HttpPost]
		[Authorize(Roles = "User,Editor,Admin")]
		public IActionResult AddComment([FromForm] Comment comment)
		{
			comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
			{
				db.Comments.Add(comment);
				db.SaveChanges();
                TempData["message"] = "Comentariul a fost adaugat!";
                return Redirect("/Articles/Show/" +
				comment.ArticleId);
			}
			else
			{
				Article art =
				db.Articles.Include("Category").Include("Comments")
				.Where(art => art.Id ==
				comment.ArticleId)
				.First();
				TempData["ShowNewComment"] = true;
                TempData["error"] = "Comentariul nu a putut fi adaugat!";
                return View("Show", art);
			}
		}

		[HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult EditComment([FromForm] Comment comment)
        {
			comment.Date = DateTime.Now;
            if (ModelState.IsValid)
            {

				if(comment.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                {
                    TempData["error"] = "Nu aveti dreptul sa editati acest comentariu!";
                    return RedirectToAction("Index");
                }

                db.Comments.Update(comment);
                db.SaveChanges();
				TempData["message"] = "Comentariul a fost modificat!";
                return Redirect("/Articles/Show/" +
                comment.ArticleId);
            }
            else
            {
                Article art =
                db.Articles.Include("Category").Include("Comments")
                .Where(art => art.Id ==
                comment.ArticleId)
                .First();
                TempData["ShowEditComment"] = comment.Id;
				TempData["LastCommentContent"] = comment.Content;
                TempData["error"] = "Comentariul nu a putut fi modificat!";
                return View("Show", art);
            }
        }

		private void SetAccessRights(string checkForId)
		{
			ViewBag.ShowButtons = User.IsInRole("Admin") || checkForId == _userManager.GetUserId(User);
        }
    }
}
