using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace V_Vuelos.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult IniciarSesion()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoginComercial(string correo, string contrasena)
        {
            try
            {
                using (Models.V_vuelos_ProyectoEntities db = new Models.V_vuelos_ProyectoEntities())
                {
                    var ToUser = (from u in db.Usuarios
                                  where u.Correo == correo.Trim() && u.Contrasenia == contrasena.Trim()
                                  select u).FirstOrDefault();
                    if (ToUser == null)
                    {
                        ViewBag.Error = "Usuario o contraseña Incorrecto";
                        return View("IniciarSesion");
                    }
                    Session["User"] = ToUser;
                }
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return RedirectToAction("Error");
            }
        }
        public ActionResult BuscaVuelosLlegada()
        {
            return View();
        }
        public ActionResult BuscaVuelosSalida()
        {
            return View();
        }
        public ActionResult CambiarContrasenna()
        {
            return View();
        }
        public ActionResult ComprarVuelosSinInicio()
        {
            return View();
        }

        public ActionResult ComprarVuelosInicio()
        {
            return View();
        }
        
        public ActionResult CrearCuenta()
        {
            return View();
        }
        public ActionResult Pagar()
        {
            return View();
        }

        public ActionResult MetodoPago()
        {
            return View();
        }

        public ActionResult LogOut()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");

        }
    }
}