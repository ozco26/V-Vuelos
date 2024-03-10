using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using V_Vuelos.Filter;
using V_Vuelos.Models;


namespace V_Vuelos.Controllers
{
    public class AdminController : Controller
    {
        private V_vuelos_ProyectoEntities _context;

        public AdminController()
        {
            _context = new V_vuelos_ProyectoEntities(); // ApplicationDbContext es tu contexto de base de datos
        }

        // GET: Admin
        //Visualiza la pantalla Bienvenidos
        [AuthorizeUser(idOperation:1)]
        public ActionResult Default()
        {
            return View();
        }
        //Metoto cerrar sesion
        public ActionResult LogOut()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");

        }

        public ActionResult VerUsuarios()
        {
            var usuarios = _context.Usuarios.ToList();
            return View(usuarios);
        }

        [HttpPost]
        public ActionResult ActualizarRoles(int usuarioId, int rol)
        {
            try
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.Id_Usuario == usuarioId);
                usuario.Rol = rol;
                _context.SaveChanges();

                // Lógica para actualizar los roles en la base de datos...

                // Por ahora, simplemente devolvemos un código de éxito
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Manejar cualquier error que ocurra durante la actualización de roles
                return Json(new { success = false, message = ex.Message });
            }
        }

    }

}