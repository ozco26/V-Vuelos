using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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
        private readonly V_vuelos_ProyectoEntities _context;

        public AdminController() => _context = new V_vuelos_ProyectoEntities(); // ApplicationDbContext es tu contexto de base de datos

        //Visualiza la pantalla Bienvenidos
        [AuthorizeUser(idOperation: 1)]
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

        [AuthorizeUser(idOperation: 2)]
        public ActionResult VerUsuarios()
        {
            var usuarios = _context.Usuarios.ToList();
            return View(usuarios);
        }

        [AuthorizeUser(idOperation: 2)]
        [HttpPost]
        public ActionResult ActualizarRoles(int usuarioId, int rol)
        {
            try
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.Id_Usuario == usuarioId);
                usuario.Rol = rol;
                _context.SaveChanges();

                //Devolvemos un código de éxito
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Manejar cualquier error que ocurra durante la actualización de roles
                return Json(new { success = false });
            }
        }

        [AuthorizeUser(idOperation: 2)]
        public ActionResult CrearUsuario()
        {
            return View();
        }

        [AuthorizeUser(idOperation: 2)]
        [HttpPost]
        public ActionResult CrearUsuarioNuevo(Usuario nuevoUsuario)
        {
            try
            {
                // Verifica si el nombre de usuario ya está en uso
                var usuarioExistente = _context.Usuarios.FirstOrDefault(u => u.Nombre_Usuario == nuevoUsuario.Nombre_Usuario);
                if (usuarioExistente != null)
                {
                    return Json(new { success = false, message = "El nombre de usuario ya está en uso." });
                }

                // Agregar nuevo usuario a la base de datos
                _context.Usuarios.Add(nuevoUsuario);
                _context.SaveChanges();

                // Devolver un código de éxito
                return Json(new { success = true, message = "Usuario creado correctamente." });
            }
            catch (DbEntityValidationException ex)
            {
                // Manejar errores de validación del modelo
                var errorMessage = ex.EntityValidationErrors
                                      .SelectMany(x => x.ValidationErrors)
                                      .Select(x => x.ErrorMessage);
                return Json(new { success = false, message = "Error al crear usuario: " + ex.Message });
            }
            catch (Exception ex)
            {
                // Manejar otros errores
                return Json(new { success = false, message = "Error al crear el usuario: " + ex.Message });
            }
        }

        [AuthorizeUser(idOperation: 1)]
        public ActionResult CambiarContrasenna()
        {
            return View();
        }

        [AuthorizeUser(idOperation: 1)]
        public ActionResult GeneraCambioContrasenna(string Nombre_Usuario, string ContraseniaAnterior, string ContraseniaActual)
        {
            try
            {
                var usuarioExistente = _context.Usuarios.FirstOrDefault(u => u.Nombre_Usuario == Nombre_Usuario);
                if (usuarioExistente != null)
                {
                    // El usuario existe, ahora verifica si el password es igual al anterior por seguridad
                    if (usuarioExistente.Contrasenia == ContraseniaAnterior)
                    {
                        usuarioExistente.Contrasenia = ContraseniaActual;
                        
                        _context.SaveChanges();
                        // El password es igual, se cambia
                        return Json(new { success = true, message = "La contraseña ha sido cambiada correctamente." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "La contraseña actual no coincide con la registrada, vuelva a intentar." });
                    }
                }
                else
                {
                    // El usuario no existe, puedes manejarlo de acuerdo a tus requerimientos
                    return Json(new { success = false, message = "El usuario no existe." });
                }
            }
            catch (DbEntityValidationException ex)
            {
                // Manejar errores de validación del modelo
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                return Json(new { success = false, message = exceptionMessage });
            }
            catch (Exception ex)
            {
                var errorMessages = ex.InnerException;
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", errorMessages);
                
                return Json(new { success = false, message = "Error al editar contraseña: " + exceptionMessage });
            }
        }






        public ActionResult LstConsecutivos()
        {
            return View();
        }
        public ActionResult EditConsecutivo()
        {
            return View();
        }

        public ActionResult Paises()
        {
            return View();
        }

        public ActionResult Aerolineas()
        {
            return View();
        }

        public ActionResult Puertas()
        {
            return View();
        }
        public ActionResult Bitacora()
        {
            return View();
        }

        public ActionResult Errores()
        {
            return View();
        }

        public ActionResult AerolineasPorPais()
        {
            return View();
        }
        public ActionResult EstadoPuertas()
        {
            return View();
        }
    }
}

