using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;




namespace V_Vuelos.Controllers
{
    public class AccesoAdministrativosController : Controller
    {
        // GET: AccesoAdministrativos
        public ActionResult LoginAdministrativo()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            try
            {
                using (Models.V_vuelos_ProyectoEntities db = new Models.V_vuelos_ProyectoEntities())
                {
                    var ToUser = (from u in db.Usuarios
                                 where u.Nombre_Usuario == username.Trim() &&  u.Contrasenia == password.Trim()
                                 select u).FirstOrDefault();
                    if (ToUser == null)
                    {
                        ViewBag.Error = "Usuario o contraseña Incorrecto";
                        return View("LoginAdministrativo");
                    }
                    Session["User"] = ToUser;
                }
                return RedirectToAction("Default", "Admin");
            }
            catch(Exception ex)
            {
                ViewBag.Error = ex.Message;
                return RedirectToAction("Error");
            }
        }
    }
}