using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
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

        public AdminController() => _context = new V_vuelos_ProyectoEntities(); //  contexto de base de datos

        private void RegistrarError(Exception ex , string controlador,string accion)
        {
            using (var context = new V_vuelos_ProyectoEntities())
            {
                DateTime fechaActual = DateTime.Now;
                var errorLog = new Errore
                {
                    Descripcion = "Error producido en "+controlador+ " "+accion,
                    Fecha_Hora = fechaActual,
                    Error = ex.Message
                };
                // Agregar el error a la base de datos y guardar los cambios, fecha e id los caoloca la BD
                context.Errores.Add(errorLog);
                context.SaveChanges();
            }
        }
    


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



        //Usuarios




        //Metodo cargar pagina lsitando usuarios
        [AuthorizeUser(idOperation: 2)]
        public async Task<ActionResult> VerUsuarios()
        {

            string ConsultaApi = "http://localhost:5056/api/Values/VerUsuarios";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ConsultaApi);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta JSON y convertirla en una lista de objetos Aerolinea
                    var json = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var model = JsonConvert.DeserializeObject<List<Usuario>>(json);

                        return View(model);
                    }
                    catch (JsonReaderException ex)
                    {
                        // Manejar la excepción de deserialización JSON
                        Console.WriteLine("Error al deserializar el JSON: " + ex.Message);
                        return View("Error", "Error al procesar los datos recibidos.");
                    }
                }
                else
                {
                    // Si la solicitud no fue exitosa, devolver una vista de error
                    return View("Error");
                }
            }
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
                RegistrarError(ex, this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
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

        //vista de CAmbiar Contraseña
        [AuthorizeUser(idOperation: 1)]
        public ActionResult CambiarContrasenna()
        {
            return View();
        }


        //Consecutivos



        [AuthorizeUser(idOperation: 3)]
        public async Task<ActionResult> LstConsecutivos()
        {
            string ConsultaApi = "http://localhost:5056/api/Values/Obtener-Consecutivos";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ConsultaApi);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta JSON y convertirla en una lista de objetos Aerolinea
                    var json = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var model = JsonConvert.DeserializeObject<List<Consecutivos>>(json);

                        return View(model);
                    }
                    catch (JsonReaderException ex)
                    {
                        // Manejar la excepción de deserialización JSON
                        Console.WriteLine("Error al deserializar el JSON: " + ex.Message);
                        return View("Error", "Error al procesar los datos recibidos.");
                    }
                }
                else
                {
                    // Si la solicitud no fue exitosa, devolver una vista de error
                    return View("Error");
                }
            }
            
        }


        public async Task<ActionResult> EditConsecutivo(int id)
        {
            string ConsultaApi = $"http://localhost:5056/api/Values/Obtener-Consecutivo/{id}";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ConsultaApi);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta JSON y convertirla en un objeto Consecutivo
                    var json = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var consecutivo = JsonConvert.DeserializeObject<Consecutivos>(json);

                        if (consecutivo == null)
                        {
                            return RedirectToAction("LstConsecutivos", "Admin"); // Redireccionar a la página principal
                        }

                        return View(consecutivo);
                    }
                    catch (JsonReaderException ex)
                    {
                        // Manejar la excepción de deserialización JSON
                        Console.WriteLine("Error al deserializar el JSON: " + ex.Message);
                        return View("Error", "Error al procesar los datos recibidos.");
                    }
                }
                else
                {
                    // Si la solicitud no fue exitosa, devolver una vista de error
                    return View("Error");
                }
            }
        }


        /*
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
                    descripcionBD.Consecutivo = Consecutivo1;
                    descripcionBD.PoseeRango = PoseeRango;
                    descripcionBD.RangoInicial = RangoInicial;
                    descripcionBD.RangoFinal = RangoFinal;

                    _context.SaveChanges();
                    // El password es igual, se cambia
                    return Json(new { success = true, message = "La tabla Consecutivo ha sido Modificada correctamente." });
                }
                else
                {
                    try
                    {
                        throw new Exception("La Descripcion actual no coincide con las registrada, vuelva a intentar.");
                    }
                    catch (Exception ex)
                    {
                        RegistrarError(ex, this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
                        return Json(new { success = false, message = "La Descripcion actual no coincide con las registrada, vuelva a intentar." });
                    }

                    
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
                RegistrarError(ex, this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
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
                RegistrarError(ex, this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
                return Json(new { success = false, message = "Error al editar contraseña: " + exceptionMessage });
            }
        }

        [AuthorizeUser(idOperation: 3)]
        [HttpPost]
        public ActionResult GuardarAero(Aerolinea nuevoAero)
        {
            try
            {
                var usuarioExistente = _context.Aerolineas.FirstOrDefault(u => u.NombreAgencia == nuevoAero.NombreAgencia);
                if (usuarioExistente != null)
                {
                    try
                    {
                        throw new Exception("Ya existe una aerolinea con el mismo nombre.");
                    }
                    catch (Exception ex)
                    {
                        //RegistrarError(ex, this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
                        return Json(new { success = false, message = "Ya existe una aerolinea con el mismo nombre: "+ex });
                    }
                }

                var ActualConsecutivoPais = _context.Consecutivos;

                var consecutivoPaises = ActualConsecutivoPais.Where(c => c.Descripcion == "aerolineas");
                var ultimoConsecutivo = consecutivoPaises.OrderByDescending(c => c.Consecutivo1).FirstOrDefault();
                var ultimoPrefijoPais = ultimoConsecutivo?.Prefijo;

                var numeroConsecutivo = ultimoConsecutivo != null ? Convert.ToInt32(ultimoConsecutivo.Consecutivo1) + 1 : 1;

                var nuevoConsecutivo = new Consecutivo
                {
                    Descripcion = "aerolineas", // Establecer la descripción
                    Prefijo = ultimoConsecutivo?.Prefijo, // Establecer el prefijo (utilizando el operador de navegación segura)
                    PoseePrefijo = ultimoConsecutivo?.PoseePrefijo ?? false,
                    Consecutivo1 = numeroConsecutivo,
                    PoseeRango = ultimoConsecutivo?.PoseeRango ?? false, // Establecer si posee rango (utilizando el operador de navegación segura)
                    RangoInicial = ultimoConsecutivo?.RangoInicial, // Establecer el rango inicial (utilizando el operador de navegación segura)
                    RangoFinal = ultimoConsecutivo?.RangoFinal // Establecer el rango final (utilizando el operador de navegación segura)
                };

                // Agregar nuevo usuario a la base de datos
                _context.Aerolineas.Add(nuevoAero);
                _context.Consecutivos.Add(nuevoConsecutivo);
                _context.SaveChanges();

                // Devolver un código de éxito
                return Json(new { success = true, message = "Aerolinea registrada con exito" });
            }
            catch (DbEntityValidationException ex)
            {
                // Obtener los mensajes de error de validación
                var errorMessages = ex.EntityValidationErrors
                                      .SelectMany(x => x.ValidationErrors)

                                      .Select(x => x.ErrorMessage);

                // Combinar los mensajes de error en una sola cadena
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Devolver los mensajes de error
                //RegistrarError(ex, this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
                return Json(new { success = false, message = "Error al registrar Aerolinea: " + fullErrorMessage });
            }


            //return Json(new { success = true, message = nuevoAero.ToString()});
        }
        
        [AuthorizeUser(idOperation: 3)]
        [HttpPost]
        public ActionResult GuardarImagen(Pai nuevoPais)
        {

            try
            {
                
                var usuarioExistente = _context.Pais.FirstOrDefault(u => u.NombrePais == nuevoPais.NombrePais);
                if (usuarioExistente != null)
                {
                    try
                    {
                        throw new Exception("Ya existe un país con el mismo nombre.");
                    }
                    catch (Exception ex)
                    {
                        RegistrarError(ex, this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
                        return Json(new { success = false, message = "Ya existe un país con el mismo nombre." });
                    }
                    
                }

                var ActualConsecutivoPais = _context.Consecutivos;

                var consecutivoPaises = ActualConsecutivoPais.Where(c => c.Descripcion == "paises");
                var ultimoConsecutivo = consecutivoPaises.OrderByDescending(c => c.Consecutivo1).FirstOrDefault();
                var ultimoPrefijoPais = ultimoConsecutivo?.Prefijo;

                var numeroConsecutivo = ultimoConsecutivo != null ? Convert.ToInt32(ultimoConsecutivo.Consecutivo1) + 1 : 1;



                var nuevoConsecutivo = new Consecutivo
                {
                    Descripcion = "paises", // Establecer la descripción
                    Prefijo = ultimoConsecutivo?.Prefijo, // Establecer el prefijo (utilizando el operador de navegación segura)
                    PoseePrefijo = ultimoConsecutivo?.PoseePrefijo ?? false, 
                    Consecutivo1 = numeroConsecutivo,
                    PoseeRango = ultimoConsecutivo?.PoseeRango ?? false, // Establecer si posee rango (utilizando el operador de navegación segura)
                    RangoInicial = ultimoConsecutivo?.RangoInicial, // Establecer el rango inicial (utilizando el operador de navegación segura)
                    RangoFinal = ultimoConsecutivo?.RangoFinal // Establecer el rango final (utilizando el operador de navegación segura)
                };



                // Agregar nuevo usuario a la base de datos
                _context.Pais.Add(nuevoPais);
                _context.Consecutivos.Add(nuevoConsecutivo);
                _context.SaveChanges();

                // Devolver un código de éxito
                return Json(new { success = true, message = "Pais registrado con exito" });
            }
            catch (DbEntityValidationException ex)
            {
                // Obtener los mensajes de error de validación
                var errorMessages = ex.EntityValidationErrors
                                      .SelectMany(x => x.ValidationErrors)
                                      .Select(x => x.ErrorMessage);

                // Combinar los mensajes de error en una sola cadena
                var fullErrorMessage = string.Join("; ", errorMessages);
                RegistrarError(ex, this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
                // Devolver los mensajes de error
                return Json(new { success = false, message = "Error al registrar el Pais: " + fullErrorMessage });
            }

            //return Json(new { success = true, message = "Se guardó la imagen"});
        }
        
        [AuthorizeUser(idOperation: 3)]
        [HttpPost]
        public ActionResult GuardarPuerta(Puerta nuevaPuerta)
        {

            try
            {

                var usuarioExistente = _context.Puertas.FirstOrDefault(u => u.numero == nuevaPuerta.numero);
                if (usuarioExistente != null)
                {
                    try
                    {
                        throw new Exception("La puerta ya está registrada");
                    }
                    catch (Exception ex)
                    {
                        RegistrarError(ex, this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
                        return Json(new { success = false, message = "La puerta ya está registrada" });
                    }
                    
                }

                var ActualConsecutivoPuerta = _context.Consecutivos;

                var consecutivoPuerta = ActualConsecutivoPuerta.Where(c => c.Descripcion == "puertas");
                var ultimoConsecutivo = consecutivoPuerta.OrderByDescending(c => c.Consecutivo1).FirstOrDefault();
                var ultimoPrefijoPuerta = ultimoConsecutivo?.Prefijo;

                var numeroConsecutivo = ultimoConsecutivo != null ? Convert.ToInt32(ultimoConsecutivo.Consecutivo1) + 1 : 1;

                var nuevoConsecutivo = new Consecutivo
                {
                    Descripcion = "puertas", // Establecer la descripción
                    Prefijo = ultimoConsecutivo?.Prefijo, // Establecer el prefijo (utilizando el operador de navegación segura)
                    PoseePrefijo = ultimoConsecutivo?.PoseePrefijo ?? false,
                    Consecutivo1 = numeroConsecutivo,
                    PoseeRango = ultimoConsecutivo?.PoseeRango ?? false, // Establecer si posee rango (utilizando el operador de navegación segura)
                    RangoInicial = ultimoConsecutivo?.RangoInicial, // Establecer el rango inicial (utilizando el operador de navegación segura)
                    RangoFinal = ultimoConsecutivo?.RangoFinal // Establecer el rango final (utilizando el operador de navegación segura)
                };

                // Agregar nuevo usuario a la base de datos
                _context.Puertas.Add(nuevaPuerta);
                _context.Consecutivos.Add(nuevoConsecutivo);
                _context.SaveChanges();

                // Devolver un código de éxito
                return Json(new { success = true, message = "Puerta registrada con exito" });
            }
            catch (DbEntityValidationException ex)
            {
                // Obtener los mensajes de error de validación
                var errorMessages = ex.EntityValidationErrors
                                      .SelectMany(x => x.ValidationErrors)
                                      .Select(x => x.ErrorMessage);

                // Combinar los mensajes de error en una sola cadena
                var fullErrorMessage = string.Join("; ", errorMessages);
                RegistrarError(ex, this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
                // Devolver los mensajes de error
                return Json(new { success = false, message = "Error al registrar Puerta: " + fullErrorMessage });
            }

            //return Json(new { success = true, message = "Se guardó la imagen"});
        }
        
        [AuthorizeUser(idOperation: 3)]
        [HttpPost]
        public ActionResult CambiarEstado(string codigoPuerta)
        {
            try
            {
                var puerta = _context.Puertas.FirstOrDefault(p => p.Codigo == codigoPuerta);
                if (puerta == null)
                {
                    try
                    {
                        throw new Exception("La puerta no existe");
                    }
                    catch (Exception ex)
                    {
                        RegistrarError(ex, this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
                        return Json(new { success = false, message = "La puerta no existe" });
                    }
                    
                }

                // Cambiar el estado de la puerta
                puerta.Detalle = puerta.Detalle == "1" ? "0" : "1";
                _context.SaveChanges();

                // Devolver un código de éxito
                return Json(new { success = true, message = "Estado de la puerta cambiado con éxito" });
            }
            catch (Exception ex)
            {

                RegistrarError(ex, this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
                return Json(new { success = false, message = "Error al cambiar el estado de la puerta: " + ex.Message });
            }
        }
        */
        /*
        [AuthorizeUser(idOperation: 3)]
        public ActionResult Paises()
        {
            var paises = _context.Pais;
            var consecutivo = _context.Consecutivos;

            var consecutivoPaises = consecutivo.Where(c => c.Descripcion == "paises");
            var ultimoConsecutivo = consecutivoPaises.OrderByDescending(c => c.Consecutivo1).FirstOrDefault();
            var ultimoPrefijoPais = ultimoConsecutivo?.Prefijo;
            ViewBag.UltimoPrefijoPais = ultimoPrefijoPais;
            ViewBag.NumeroConsecutivo = ultimoConsecutivo?.Consecutivo1;

            ViewBag.Paises = paises;
            return View();
        }

        //Aerolinea
        [AuthorizeUser(idOperation: 3)]
        /
        public ActionResult Aerolineas()
        {

            var aerolinea = _context.Aerolineas;
            var consecutivo = _context.Consecutivos;

            var consecutivoAero = consecutivo.Where(c => c.Descripcion == "aerolineas");
            var ultimoConsecutivo = consecutivoAero.OrderByDescending(c => c.Consecutivo1).FirstOrDefault();
            var ultimoPrefijoAero = ultimoConsecutivo?.Prefijo;
            ViewBag.UltimoPrefijoAero = ultimoPrefijoAero;
            ViewBag.NumeroConsecutivo = ultimoConsecutivo?.Consecutivo1;

            ViewBag.Aerolinea = aerolinea;

            return View();
        }
        */
        /*
        public ActionResult EstadoPuertas()
        {
            var puerta = _context.Puertas;
            var consecutivo = _context.Consecutivos;

            var consecutivoPuerta = consecutivo.Where(c => c.Descripcion == "puertas");
            var ultimoConsecutivo = consecutivoPuerta.OrderByDescending(c => c.Consecutivo1).FirstOrDefault();
            var ultimoPrefijoPuerta = ultimoConsecutivo?.Prefijo;

            ViewBag.UltimoPrefijoPuerta = ultimoPrefijoPuerta;
            ViewBag.NumeroConsecutivo = ultimoConsecutivo?.Consecutivo1;

            ViewBag.puerta = puerta;

            return View();
        }
        */






        //Aerolineas







        public async Task<ActionResult> Aerolineas()
        {
            try
            {
                string ConsultaApi = "http://localhost:5056/api/Values/Obtener-Aerolineas";

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(ConsultaApi);

                    // Verificar si la solicitud fue exitosa
                    if (response.IsSuccessStatusCode)
                    {
                        // Leer la respuesta JSON y convertirla en una lista de objetos Aerolinea
                        var json = await response.Content.ReadAsStringAsync();

                        try
                        {
                            var aerolineas = JsonConvert.DeserializeObject<List<Aerolinea>>(json);
                            Console.WriteLine("aerolineas: "+aerolineas.Count);
                            // Obtener los datos de consecutivo de aerolíneas de la API o de la base de datos, según corresponda
                            
                            var ultimoPrefijoAero = await ObtenerUltimoPrefijoAeroDesdeAPI();
                            var numeroConsecutivo = await ObtenerNumeroConsecutivoDesdeAPI();

                            Console.WriteLine("Error: "+ultimoPrefijoAero+" y otro error: "+numeroConsecutivo);

                            // Asignar los datos al ViewBag
                            ViewBag.UltimoPrefijoAero = ultimoPrefijoAero;
                            ViewBag.NumeroConsecutivo = numeroConsecutivo;
                            ViewBag.Aerolineas = aerolineas;

                            // Devolver la vista con los datos
                            return View(ViewBag);
                        }
                        catch (JsonReaderException ex)
                        {
                            // Manejar la excepción de deserialización JSON
                            Console.WriteLine("Error al deserializar el JSON: " + ex.Message);
                            return View("Error", "Error al procesar los datos recibidos.");
                        }
                    }
                    else
                    {
                        // Si la solicitud no fue exitosa, devolver una vista de error
                        return View("Error");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Capturar excepción relacionada con la solicitud HTTP
                Console.WriteLine("Error al realizar la solicitud HTTP: " + ex.Message);
                return View("Error", ex.Message);
            }
            catch (Exception ex)
            {
                // Capturar cualquier otra excepción no especificada
                Console.WriteLine("Error inesperado: " + ex.Message);
                return View("Error");
            }
        }

        private async Task<string> ObtenerUltimoPrefijoAeroDesdeAPI()
        {
            string ConsultaApi = "http://localhost:5056/api/Values/Prefijo-AeroLinea";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ConsultaApi);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta como una cadena
                    var json = await response.Content.ReadAsStringAsync();
                    return json.ToString();
                }
                else
                {
                    throw new Exception("La solicitud a la API para obtener el último prefijo de aerolíneas falló.");
                }
            }
        }

        private async Task<int> ObtenerNumeroConsecutivoDesdeAPI()
        {
            string ConsultaApi = "http://localhost:5056/api/Values/Consecutivo-Aerolinea";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ConsultaApi);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta como una cadena
                    var json = await response.Content.ReadAsStringAsync();

                    try
                    {
                        // Deserializar la cadena JSON en un objeto int
                        var resultado = JsonConvert.DeserializeObject<int>(json);
                        return resultado;
                    }
                    catch (JsonReaderException ex)
                    {
                        // Manejar la excepción de deserialización JSON
                        throw new Exception("Error al deserializar el número consecutivo de aerolíneas desde la API.", ex);
                    }
                }
                else
                {
                    throw new Exception("La solicitud a la API para obtener el número consecutivo de aerolíneas falló.");
                }
            }
        }



        //Paises



        public async Task<ActionResult> Paises()
        {
            try
            {
                string ConsultaApi = "http://localhost:5056/api/Values/Obtener-Paises";

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(ConsultaApi);

                    // Verificar si la solicitud fue exitosa
                    if (response.IsSuccessStatusCode)
                    {
                        // Leer la respuesta JSON y convertirla en una lista de objetos Aerolinea
                        var json = await response.Content.ReadAsStringAsync();

                        try
                        {
                            var paises = JsonConvert.DeserializeObject<List<Pai>>(json);
                           // Console.WriteLine("aerolineas: " + aerolineas.Count);
                            // Obtener los datos de consecutivo de aerolíneas de la API o de la base de datos, según corresponda

                            var ultimoPrefijoPais = await ObtenerUltimoPrefijoPaisDesdeAPI();
                            var numeroConsecutivo = await ObtenerNumeroConsecutivoDesdePaisAPI();

                            //Console.WriteLine("Error: " + ultimoPrefijoPais + " y otro error: " + numeroConsecutivo);

                            // Asignar los datos al ViewBag
                            ViewBag.UltimoPrefijoPais = ultimoPrefijoPais;
                            ViewBag.NumeroConsecutivo = numeroConsecutivo;
                            ViewBag.paises = paises;

                            // Devolver la vista con los datos
                            return View(ViewBag);
                        }
                        catch (JsonReaderException ex)
                        {
                            // Manejar la excepción de deserialización JSON
                            Console.WriteLine("Error al deserializar el JSON: " + ex.Message);
                            return View("Error", "Error al procesar los datos recibidos.");
                        }
                    }
                    else
                    {
                        // Si la solicitud no fue exitosa, devolver una vista de error
                        return View("Error");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Capturar excepción relacionada con la solicitud HTTP
                Console.WriteLine("Error al realizar la solicitud HTTP: " + ex.Message);
                return View("Error", ex.Message);
            }
            catch (Exception ex)
            {
                // Capturar cualquier otra excepción no especificada
                Console.WriteLine("Error inesperado: " + ex.Message);
                return View("Error");
            }
        }

        private async Task<string> ObtenerUltimoPrefijoPaisDesdeAPI()
        {
            string ConsultaApi = "http://localhost:5056/api/Values/Prefijo-Paises";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ConsultaApi);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta como una cadena
                    var json = await response.Content.ReadAsStringAsync();
                    return json.ToString();
                }
                else
                {
                    throw new Exception("La solicitud a la API para obtener el último prefijo de paises falló.");
                }
            }
        }

        private async Task<int> ObtenerNumeroConsecutivoDesdePaisAPI()
        {
            string ConsultaApi = "http://localhost:5056/api/Values/Consecutivo-Paises";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ConsultaApi);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta como una cadena
                    var json = await response.Content.ReadAsStringAsync();

                    try
                    {
                        // Deserializar la cadena JSON en un objeto int
                        var resultado = JsonConvert.DeserializeObject<int>(json);
                        return resultado;
                    }
                    catch (JsonReaderException ex)
                    {
                        // Manejar la excepción de deserialización JSON
                        throw new Exception("Error al deserializar el número consecutivo de aerolíneas desde la API.", ex);
                    }
                }
                else
                {
                    throw new Exception("La solicitud a la API para obtener el número consecutivo de paises falló.");
                }
            }
        }


        public async Task<ActionResult> EstadoPuertas()
        {
            try
            {
                string ConsultaApi = "http://localhost:5056/api/Values/Obtener-Puertas";

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(ConsultaApi);

                    // Verificar si la solicitud fue exitosa
                    if (response.IsSuccessStatusCode)
                    {
                        // Leer la respuesta JSON y convertirla en una lista de objetos Aerolinea
                        var json = await response.Content.ReadAsStringAsync();

                        try
                        {
                            var aerolineas = JsonConvert.DeserializeObject<List<Puerta>>(json);
                            Console.WriteLine("Puerta: " + aerolineas.Count);
                            // Obtener los datos de consecutivo de aerolíneas de la API o de la base de datos, según corresponda

                            var ultimoPrefijoPuerta = await ObtenerUltimoPrefijoPuertaDesdeAPI();
                            var numeroConsecutivo = await ObtenerNumeroConsecutivoPuertaDesdeAPI();

                            Console.WriteLine("Error: " + ultimoPrefijoPuerta + " y otro error: " + numeroConsecutivo);

                            // Asignar los datos al ViewBag
                            ViewBag.UltimoPrefijoPuerta = ultimoPrefijoPuerta;
                            ViewBag.NumeroConsecutivo = numeroConsecutivo;
                            ViewBag.Puerta = aerolineas;

                            // Devolver la vista con los datos
                            return View(ViewBag);
                        }
                        catch (JsonReaderException ex)
                        {
                            // Manejar la excepción de deserialización JSON
                            Console.WriteLine("Error al deserializar el JSON: " + ex.Message);
                            return View("Error", "Error al procesar los datos recibidos.");
                        }
                    }
                    else
                    {
                        // Si la solicitud no fue exitosa, devolver una vista de error
                        return View("Error");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Capturar excepción relacionada con la solicitud HTTP
                Console.WriteLine("Error al realizar la solicitud HTTP: " + ex.Message);
                return View("Error", ex.Message);
            }
            catch (Exception ex)
            {
                // Capturar cualquier otra excepción no especificada
                Console.WriteLine("Error inesperado: " + ex.Message);
                return View("Error");
            }
        }

        private async Task<string> ObtenerUltimoPrefijoPuertaDesdeAPI()
        {
            string ConsultaApi = "http://localhost:5056/api/Values/Prefijo-Puerta";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ConsultaApi);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta como una cadena
                    var json = await response.Content.ReadAsStringAsync();
                    return json.ToString();
                }
                else
                {
                    throw new Exception("La solicitud a la API para obtener el último prefijo de aerolíneas falló.");
                }
            }
        }

        private async Task<int> ObtenerNumeroConsecutivoPuertaDesdeAPI()
        {
            string ConsultaApi = "http://localhost:5056/api/Values/Consecutivo-Puerta";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ConsultaApi);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta como una cadena
                    var json = await response.Content.ReadAsStringAsync();

                    try
                    {
                        // Deserializar la cadena JSON en un objeto int
                        var resultado = JsonConvert.DeserializeObject<int>(json);
                        return resultado;
                    }
                    catch (JsonReaderException ex)
                    {
                        // Manejar la excepción de deserialización JSON
                        throw new Exception("Error al deserializar el número consecutivo de aerolíneas desde la API.", ex);
                    }
                }
                else
                {
                    throw new Exception("La solicitud a la API para obtener el número consecutivo de aerolíneas falló.");
                }
            }
        }



        /*Errores

        public ActionResult Errores()
        {

            // Obtener la lista de errores desde la base de datos
            var errores = _context.Errores.ToList();
            return View(errores);

        }
        */

        public async Task<ActionResult> Errores()
        {
            string ConsultaApi = "http://localhost:5056/api/Values/Obtener-Errores";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ConsultaApi);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta JSON y convertirla en una lista de objetos Aerolinea
                    var json = await response.Content.ReadAsStringAsync();

                    try
                    {

                        var Errores = JsonConvert.DeserializeObject<List<Errore>>(json);
                        


                        // Devolver la vista con los datos
                        return View(Errores);
                    }
                    catch (JsonReaderException ex)
                    {
                        // Manejar la excepción de deserialización JSON
                        Console.WriteLine("Error al deserializar el JSON: " + ex.Message);
                        return View("Error", "Error al procesar los datos recibidos.");
                    }
                }
                else
                {
                    // Si la solicitud no fue exitosa, devolver una vista de error
                    return View("Error");
                }
            }

        }



        public ActionResult Bitacora()
        {
            return View();
        }

        

        public ActionResult AerolineasPorPais()
        {
            return View();
        }
       


        //Puertas


        public ActionResult Puertas()
        {
            var puerta = _context.Puertas;
            var consecutivo = _context.Consecutivos;

            var consecutivoPuerta = consecutivo.Where(c => c.Descripcion == "puertas");
            var ultimoConsecutivo = consecutivoPuerta.OrderByDescending(c => c.Consecutivo).FirstOrDefault();
            var ultimoPrefijoPuerta = ultimoConsecutivo?.Prefijo;

            ViewBag.UltimoPrefijoPuerta = ultimoPrefijoPuerta;
            ViewBag.NumeroConsecutivo = ultimoConsecutivo?.Consecutivo;

            ViewBag.puerta = puerta;

            return View();
        }
    }
}

