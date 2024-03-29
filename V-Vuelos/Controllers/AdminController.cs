using System;
using System.Collections.Generic;
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
        public ActionResult GuardarImagen(HttpPostedFileBase imagen, string codigoPais, string nombrePais)
        {
            try
            {
                if (imagen != null && imagen.ContentLength > 0)
                {
                    // Verificar si el archivo es una imagen
                    if (imagen.ContentType.ToLower().StartsWith("image/"))
                    {
                        // Convertir la imagen a bytes
                        byte[] imageData = null;
                        using (var binaryReader = new BinaryReader(imagen.InputStream))
                        {
                            imageData = binaryReader.ReadBytes(imagen.ContentLength);
                        }

                        // Crear un nuevo objeto de tipo Pais para almacenar en la base de datos
                        Pai nuevoPais = new Pai
                        {
                            CodigoPais = codigoPais,
                            NombrePais = nombrePais,
                            Imagen = Convert.ToBase64String(imageData) // Convertir los bytes de la imagen a una cadena Base64 y guardarla en la propiedad Imagen
                        };

                        // Agregar el nuevo país a la base de datos y guardar los cambios
                        _context.Pais.Add(nuevoPais);
                        _context.SaveChanges();

                        // Redirigir de vuelta a la vista de países
                        return RedirectToAction("Paises");
                    }
                    else
                    {
                        // Si el archivo no es una imagen, mostrar un mensaje de error en la consola
                        Console.WriteLine("El archivo seleccionado no es una imagen válida.");
                    }
                }
                else
                {
                    // Si no se ha seleccionado ningún archivo, mostrar un mensaje de error en la consola
                    Console.WriteLine("Por favor, seleccione una imagen.");
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier error que pueda ocurrir durante el proceso de guardar la imagen
                Console.WriteLine("Error al guardar la imagen: " + ex.Message);
            }

            // Redirigir de vuelta a la vista de países
            return RedirectToAction("Paises");
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

