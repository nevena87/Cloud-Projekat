using KorisnikService_Data;
using System.Web.Mvc;

namespace RedditService.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        private readonly LoginDataRepository _loginDataRepository;

        public LoginController()
        {
            _loginDataRepository = new LoginDataRepository();
        }

        //GET
        public ActionResult Index()
        {
            return View(new Login());
        }

        [HttpPost]
        public ActionResult Index(Login model)
        {
            if (_loginDataRepository.ValidateLogin(model.Email, model.Lozinka))
            {
                var userId = _loginDataRepository.FindByEmail(model.Email);
                Session["UserID"] = userId;
                Session["UserEmail"] = model.Email;
                return RedirectToAction("Index", "Tema");
            }
            else
            {
                ViewBag.Error = "Pogrešan email ili lozinka. Pokušajte ponovo.";
                return View(model);
            }
        }
    }
}