using ArticlesApp.Data;
using ArticlesApp.Models;
using Ganss.Xss;
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
            var articles = db.Articles.Include("Category")
                                      .Include("User")
                                      .OrderByDescending(a => a.Date);

            // ViewBag.OriceDenumireSugestiva
            // ViewBag.Articles = articles;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            // MOTOR DE CAUTARE

            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim(); // eliminam spatiile libere 

                // Cautare in articol (Title si Content)

                List<int> articleIds = db.Articles.Where
                                        (
                                         at => at.Title.Contains(search)
                                         || at.Content.Contains(search)
                                        ).Select(a => a.Id).ToList();

                // Cautare in comentarii (Content)
                List<int> articleIdsOfCommentsWithSearchString = db.Comments
                                        .Where
                                        (
                                         c => c.Content.Contains(search)
                                        ).Select(c => (int)c.ArticleId).ToList();

                // Se formeaza o singura lista formata din toate id-urile selectate anterior
                List<int> mergedIds = articleIds.Union(articleIdsOfCommentsWithSearchString).ToList();


                // Lista articolelor care contin cuvantul cautat
                // fie in articol -> Title si Content
                // fie in comentarii -> Content
                articles = db.Articles.Where(article => mergedIds.Contains(article.Id))
                                      .Include("Category")
                                      .Include("User")
                                      .OrderByDescending(a => a.Date);

            }

            ViewBag.SearchString = search;

            // AFISARE PAGINATA

            // Alegem sa afisam 3 articole pe pagina
            int _perPage = 3;

            // Fiind un numar variabil de articole, verificam de fiecare data utilizand 
            // metoda Count()

            int totalItems = articles.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            // /Articles/Index?page=valoare

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3 
            // Asadar offsetul este egal cu numarul de articole care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            // Se preiau articolele corespunzatoare pentru fiecare pagina la care ne aflam 
            // in functie de offset
            var paginatedArticles = articles.Skip(offset).Take(_perPage);


            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem articolele cu ajutorul unui ViewBag catre View-ul corespunzator
            ViewBag.Articles = paginatedArticles;

            // DACA AVEM AFISAREA PAGINATA IMPREUNA CU SEARCH

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?page";
            }

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

            // Get the Bookmarks for the current user
            var user = _userManager.GetUserAsync(User).Result;
            var bookmarks = db.Bookmarks
                            .Where(b => b.UserId == user.Id)
                            .ToList();

            ViewBag.Bookmarks = bookmarks;

            if (article == null)
			{
                TempData["error"] = "Articolul nu exista!";
                return RedirectToAction("Index");
            }

			ViewBag.CurrentUserId = _userManager.GetUserId(User);
            SetAccessRights(article.UserId);
			
			return View(article);
		}




        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult AddBookmark([FromForm] ArticleBookmark articleBookmark)
        {
            // Daca modelul este valid
            if (ModelState.IsValid)
            {
                // Verificam daca avem deja articolul in colectie
                if (db.ArticleBookmarks
                    .Where(ab => ab.ArticleId == articleBookmark.ArticleId)
                    .Where(ab => ab.BookmarkId == articleBookmark.BookmarkId)
                    .Count() > 0)
                {
                    TempData["error"] = "Acest articol este deja adaugat in colectie";
                }
                else
                {
                    // Adaugam asocierea intre articol si bookmark 
                    db.ArticleBookmarks.Add(articleBookmark);
                    // Salvam modificarile
                    db.SaveChanges();

                    // Adaugam un mesaj de succes
                    TempData["message"] = "Articolul a fost adaugat in colectia selectata";
                }

            }
            else
            {
                TempData["error"] = "Articolul nu a putut fi adaugat in colectie";
            }

            // Ne intoarcem la pagina articolului
            return Redirect("/Articles/Show/" + articleBookmark.ArticleId);
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
            var sanitizer = new HtmlSanitizer();
            article.Date = DateTime.Now;

			article.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
			{
                article.Content = sanitizer.Sanitize(article.Content);
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
            var sanitizer = new HtmlSanitizer();
            if (ModelState.IsValid)
            {
                var article = db.Articles.Find(id);

                if (article.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                {
                    TempData["error"] = "Nu aveti dreptul sa editati acest articol!";
                    return RedirectToAction("Index");
                }

                requestArticle.Content = sanitizer.Sanitize(requestArticle.Content);

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
