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
        public ActionResult IniciarSesion()
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