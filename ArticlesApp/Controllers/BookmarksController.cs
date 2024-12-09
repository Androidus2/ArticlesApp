using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Controllers
{
    public class BookmarksController : Controller
    {
        
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public BookmarksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var user = _userManager.GetUserAsync(User).Result;
			// Get the bookmarks and include the user
			var bookmarks = db.Bookmarks
							.Include(b => b.User)
							.ToList();
			ViewBag.Bookmarks = bookmarks;
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult ShowUserBookmarks()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var bookmarks = db.Bookmarks
                            .Where(b => b.UserId == user.Id)
                            .ToList();
            ViewBag.Bookmarks = bookmarks;
            return View();
        }


        [HttpGet]
		[Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
		{
			try
			{
				// Get the bookmark and include all the ArticleBookmarks and the Articles
				var bookmark = db.Bookmarks
								.Include(b => b.ArticleBookmarks)
								.ThenInclude(ab => ab.Article)
								.FirstOrDefault(b => b.Id == id);
				if (bookmark.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
				{
					TempData["error"] = "Nu aveti dreptul sa vizualizati aceasta colectie!";
					return RedirectToAction("Index");
				}
				return View(bookmark);
			}
			catch (Exception e)
			{
				TempData["error"] = "Colectia nu exista!";
				return RedirectToAction("Index");
			}
		}

		[HttpGet]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult New()
        {
            Bookmark bookmark = new Bookmark();
            bookmark.UserId = _userManager.GetUserId(User);
            return View(bookmark);
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult New(Bookmark bookmark)
        {
            bookmark.UserId = _userManager.GetUserId(User);
            try
            {
                if (ModelState.IsValid)
                {
                    db.Bookmarks.Add(bookmark);
                    db.SaveChanges();
                    TempData["message"] = "Colectia a fost adaugata!";
                    return RedirectToAction("Index");
                }

                TempData["error"] = bookmark.Name + " / " + bookmark.Id + "Datele introduse nu sunt valide!" + bookmark.UserId;
                return View(bookmark);
            }
            catch (Exception e)
            {
                TempData["error"] = "Eroare la adaugarea colectiei!";
                return View(bookmark);
            }
        }

        [HttpGet]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            try
            {
                Bookmark bookmark = db.Bookmarks.Find(id);
                if (bookmark.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                {
                    TempData["error"] = "Nu aveti dreptul sa editati aceasta colectie!";
                    return RedirectToAction("Index");
                }
                return View(bookmark);
            }
            catch (Exception e)
            {
                TempData["error"] = "Colectia nu exista!";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(Bookmark bookmark)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Bookmarks.Update(bookmark);
                    db.SaveChanges();
                    TempData["message"] = "Colectia a fost modificata!";
                    return RedirectToAction("Index");
                }
                TempData["error"] = "Datele introduse nu sunt valide!";
                return View(bookmark);
            }
            catch (Exception e)
            {
                TempData["error"] = "Eroare la modificarea colectiei!";
                return View(bookmark);
            }
        }

        [HttpGet]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                Bookmark bookmark = db.Bookmarks.Find(id);
                if (bookmark.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                {
                    TempData["error"] = "Nu aveti dreptul sa stergeti aceasta colectie!";
                    return RedirectToAction("Index");
                }

                var articleBookmarks = db.ArticleBookmarks
                                        .Where(ab => ab.BookmarkId == bookmark.Id)
                                        .ToList();
                foreach (var articleBookmark in articleBookmarks)
                {
                    db.ArticleBookmarks.Remove(articleBookmark);
                }

                db.Bookmarks.Remove(bookmark);
                db.SaveChanges();
                TempData["message"] = "Colectia a fost stearsa!";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                TempData["error"] = "Colectia nu exista!";
                return RedirectToAction("Index");
            }
        }

    }
}
