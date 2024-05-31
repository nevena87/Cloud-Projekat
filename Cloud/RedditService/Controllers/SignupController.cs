using KorisnikService_Data;
using System.Web;
using System.Web.Mvc;

namespace RedditService.Controllers
{
    public class SignupController : Controller
    {
        private readonly SignupDataRepository _signupDataRepository;

        public SignupController()
        {
            _signupDataRepository = new SignupDataRepository();
        }
        // GET: Signup
        public ActionResult Index()
        {
            return View(new Signup());
        }

        [HttpPost]
        public ActionResult Index(Signup model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var signupEntity = new Signup
                {
                    Ime = model.Ime,
                    Prezime = model.Prezime,
                    Adresa = model.Adresa,
                    Grad = model.Grad,
                    Drzava = model.Drzava,
                    BrojTelefona = model.BrojTelefona,
                    Email = model.Email,
                    Lozinka = model.Lozinka
                };
                _signupDataRepository.AddSignup(signupEntity, imageFile);
                return RedirectToAction("Index", "Login");
            }
            return View(model);
        }
    }
}