using System.Collections.Generic;
using System.Web;

namespace KorisnikService_Data
{
    public class EditViewModel
    {
        public EditViewModel()
        {
            Signup = new Signup();
            Teme = new List<Tema>();
            Tema = new Tema();
            UserTeme = new List<Tema>();
            Pretplate = new List<Pretplata>();
        }

        public Signup Signup { get; set; }
        public List<Tema> Teme { get; set; }
        public Tema Tema { get; set; }
        public List<Tema> UserTeme { get; set; }
        public List<Pretplata> Pretplate { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
    }
}
