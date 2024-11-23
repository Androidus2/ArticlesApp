using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Controllers
{
	public class ArticlesController : Controller
	{
		private readonly ApplicationDbContext db;

		public ArticlesController(ApplicationDbContext db)
		{
			this.db = db;
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

		public IActionResult Index()
		{
			var articles = db.Articles.Include(art => art.Category).ToList();
			ViewBag.Articles = articles;
			return View();
		}


		[HttpGet]
		public IActionResult Show(int id)
		{
			var article = db.Articles
							.Include(art => art.Category)
							.Include(art => art.Comments)
                            .FirstOrDefault(art => art.Id == id);

			return View(article);
		}


		[HttpGet]
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
		public IActionResult New(Article article)
		{
            article.Date = DateTime.Now;

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
		public IActionResult Edit(int id)
		{
			try
			{
				var article = db.Articles.Find(id);
				article.Categ = GetAllCategories();
				return View(article);
			}
			catch (Exception)
			{
				return RedirectToAction("Index");
			}
		}

		[HttpPost]
		public IActionResult Edit(int id, Article requestArticle)
		{

            if (ModelState.IsValid)
            {
                var article = db.Articles.Find(id);
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
		public IActionResult Delete(int id)
		{
			try
			{
				var article = db.Articles.Find(id);
				if (article != null)
				{
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
		public IActionResult AddComment([FromForm] Comment comment)
		{
			comment.Date = DateTime.Now;
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
		public IActionResult EditComment([FromForm] Comment comment)
        {
			comment.Date = DateTime.Now;
            if (ModelState.IsValid)
            {
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
    }
}
