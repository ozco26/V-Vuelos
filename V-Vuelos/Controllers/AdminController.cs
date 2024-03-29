using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI;
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

        //Metodo cargar pagina lsitando usuarios
        [AuthorizeUser(idOperation: 2)]
        public ActionResult VerUsuarios()
        {
            var usuarios = _context.Usuarios.ToList();
            return View(usuarios);
        }


        //Metodo dentro de ver usuarios, para cambiar rol en BD
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


        //carga pagina de crear usuario 
        [AuthorizeUser(idOperation: 2)]
        public ActionResult CrearUsuario()
        {
            return View();
        }


        //verifica la contraseña con la que el usuario introdujo para despues cargar en BD
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

        //vista de CAmbiar Contraseña
        [AuthorizeUser(idOperation: 1)]
        public ActionResult CambiarContrasenna()
        {
            return View();
        }

        //busca el usuario, comprueba password con BD y guarda en BD
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

        [AuthorizeUser(idOperation: 3)]
        public ActionResult LstConsecutivos()
        {
            var model = _context.Consecutivos
                       .GroupBy(c => c.Descripcion)
                       .Select(g => g.OrderByDescending(c => c.Codigo).FirstOrDefault())
                       .ToList();

            return View(model);
        }

        [AuthorizeUser(idOperation: 3)]
        public ActionResult EditConsecutivo(int id)
        {
            var consecutivo = _context.Consecutivos.FirstOrDefault(c => c.Codigo == id);
            if (consecutivo == null)
            {
                // Manejar el caso donde el consecutivo no existe, por ejemplo, mostrar un mensaje de error o redirigir a otra página
                return RedirectToAction("LstConsecutivos", "Admin"); // Redireccionar a la página principal
            }
            return View(consecutivo);
        }

        [AuthorizeUser(idOperation: 3)]
        [HttpPost]
        public ActionResult EditaLineaConsecutivo(string Descripcion, bool PoseePrefijo, string Prefijo, int Consecutivo1, bool PoseeRango, int RangoInicial, int RangoFinal)
        {
            try
            {
                var descripcionBD = _context.Consecutivos.FirstOrDefault(d => d.Descripcion == Descripcion);
                if (descripcionBD != null)
                {
                    descripcionBD.PoseePrefijo = PoseePrefijo;
                    descripcionBD.Prefijo = Prefijo;
                    descripcionBD.Consecutivo1 = Consecutivo1;
                    descripcionBD.PoseeRango = PoseeRango;
                    descripcionBD.RangoInicial = RangoInicial;
                    descripcionBD.RangoFinal = RangoFinal;

                    _context.SaveChanges();
                    // El password es igual, se cambia
                    return Json(new { success = true, message = "La tabla Consecutivo ha sido Modificada correctamente." });
                }
                else
                {
                    return Json(new { success = false, message = "La Descripcion actual no coincide con las registrada, vuelva a intentar." });
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

                return Json(new
                {
                    success = false,
                    message = exceptionMessage
                });
            }
            catch (Exception ex)
            {
                var errorMessages = ex.InnerException;
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", errorMessages);

                return Json(new { success = false, message = "Error al editar contraseña: " + exceptionMessage });
            }
        }


        [HttpPost]
        public ActionResult GuardarImagen(Pai nuevoPais)
        {

            try
            {
                // Verifica si el nombre de usuario ya está en uso
                var usuarioExistente = _context.Pais.FirstOrDefault(u => u.NombrePais == nuevoPais.NombrePais);
                if (usuarioExistente != null)
                {
                    return Json(new { success = false, message = "Ya existe un pais con el mismo nombre" });
                }

                // Agregar nuevo usuario a la base de datos
                _context.Pais.Add(nuevoPais);
                _context.SaveChanges();

                // Devolver un código de éxito
                return Json(new { success = true, message = "Pais registrado con exito" });
            }
            catch (DbEntityValidationException ex)
            {
                // Manejar errores de validación del modelo
                var errorMessage = ex.EntityValidationErrors
                                      .SelectMany(x => x.ValidationErrors)
                                      .Select(x => x.ErrorMessage);
                return Json(new { success = false, message = "Error al registrar pais: " + ex.Message });
            }
            catch (Exception ex)
            {
                // Manejar otros errores
                return Json(new { success = false, message = "Error al registrar pais: " + ex.Message });
            }

            //return Json(new { success = true, message = "Se guardó la imagen"});
        }



        public ActionResult Paises()
        {
            var paises = _context.Pais;
            ViewBag.Paises = paises;
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

