using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Controllers
{
    public class CategoriesController : Controller
    {

        private readonly ApplicationDbContext db;

        public CategoriesController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            var categories = from cat in db.Categories
                             select cat;
            ViewBag.Categories = categories;
            return View();
        }

        [HttpGet]
        public IActionResult Show(int id)
        {
            var category = db.Categories.
                Include(cat => cat.Articles).
                FirstOrDefault(cat => cat.Id == id);

			return View(category);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = db.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(int id, Category category)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    TempData["error"] = "Categoria nu a putut fi modificata!";
                    return View(category);
                }

                var cat = db.Categories.Find(id);
                if (cat != null)
                {
                    cat.CategoryName = category.CategoryName;
                    db.SaveChanges();
                }
                TempData["message"] = "Categoria a fost modificata!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["error"] = "Categoria nu a putut fi modificata!";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult New()
        {
            Category category = new Category();
            return View(category);
        }

        [HttpPost]
        public IActionResult New(Category category)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["error"] = "Categoria nu a putut fi adaugata!";
                    return View(category);
                }
                db.Categories.Add(category);
                TempData["message"] = "Categoria a fost adaugata!";
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["error"] = "Categoria nu a putut fi adaugata!";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                var category = db.Categories.Find(id);
                if(category != null)
				{
                    var articles = from art in db.Articles
								   where art.CategoryId == category.Id
								   select art;

                    foreach (var article in articles)
                    {
                        var comments = from com in db.Comments
                                       where com.ArticleId == article.Id
                                       select com;

                        foreach (var comment in comments)
                        {
                            db.Comments.Remove(comment);
                        }

						db.Articles.Remove(article);
					}

					db.Categories.Remove(category);
					db.SaveChanges();
				}
                TempData["message"] = "Categoria a fost stearsa!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["error"] = "Categoria nu a putut fi stearsa!";
                return RedirectToAction("Index");
            }
        }

    }
}
