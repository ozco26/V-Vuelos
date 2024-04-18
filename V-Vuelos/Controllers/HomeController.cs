using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using V_Vuelos.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace V_Vuelos.Controllers
{
    public class HomeController : Controller
    {
        readonly string siteKey = ConfigurationManager.AppSettings["RecaptchaSiteKey"];
        readonly string secretKey = ConfigurationManager.AppSettings["RecaptchaSecretKey"];

        public class ApiResponse
        {
            public bool success { get; set; }
            public string message { get; set; }
        }
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LoginCliente(string username, string contrasena, string captchaResponse)
        {
            try
            {
                var recaptchaValido = await ValidarReCaptcha(captchaResponse);

                if (!recaptchaValido)
                {
                    ModelState.AddModelError("", "Por favor, complete el reCAPTCHA correctamente.");
                    return View();
                }
                using (Models.V_vuelos_ProyectoEntities db = new Models.V_vuelos_ProyectoEntities())
                {
                    var ToUser = (from u in db.Usuarios
                                  where u.Nombre_Usuario == username.Trim() && u.Contrasenia == contrasena.Trim()
                                  select u).FirstOrDefault();
                    if (ToUser == null)
                    {
                        ViewBag.Error = "Usuario o contraseña Incorrecto";
                        return View("IniciarSesion");
                    }
                    Session["User"] = ToUser;
                }
                return RedirectToAction("Index", "Home"); // Redirecciona a la página de inicio después del inicio de sesión exitoso
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
        public ActionResult IniciarSesion()
        {
            return View();
        }


        public ActionResult CrearCuenta()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CrearCuenta(string nombre, string apellido1, string apellido2, string correo, string usuario, string contrasena, string confirmar, string captchaResponse) // Captura la respuesta del reCAPTCHA
        {
            // Procesar la respuesta del reCAPTCHA y los otros datos del formulario
            // Aquí debes verificar el reCAPTCHA utilizando la clave secreta
            // y luego proceder con la creación de la cuenta si la validación es exitosa

            try
            {
                var recaptchaValido = await ValidarReCaptcha(captchaResponse);
                string regexCorreo = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

                if (!recaptchaValido)
                {
                    ModelState.AddModelError("", "Por favor, complete el reCAPTCHA correctamente.");
                    return View();
                }
                
                
                // Verificar coincidencia de contraseñas
                if (contrasena != confirmar)
                {
                    ModelState.AddModelError("", "La contraseña no coincide con la confirmación.");
                    return View();
                }
                // Verificar si el correo electrónico es válido
                if (!Regex.IsMatch(correo, regexCorreo))
                {
                    ModelState.AddModelError("", "El correo electrónico no es válido.");
                    return View();
                }

                object  cuerpo1 = new  Usuario{
                Nombre_Usuario = usuario,
                Rol = 6,
                Correo = correo,
                Contrasenia = contrasena,
                PreguntaSeguridad=  nombre,
                RespuestaSeguridad=  apellido1 + apellido2,
                    };


                var httpClient = new HttpClient();
                var jsonContent = JsonConvert.SerializeObject(cuerpo1);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json"); // Redirigir a una página después de procesar la creación de la cuenta

                var response = await httpClient.PostAsync("http://localhost:5056/api/Values/CrearUsuarioNuevo", httpContent);
                response.EnsureSuccessStatusCode();

                // Leer la respuesta
                var responseData = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseData);

                // Verificar el resultado de la operación
                if (apiResponse.success)
                {
                    TempData["Mensaje"] = apiResponse.message;
                    return RedirectToAction("IniciarSesion");
                }
                else
                {
                    ModelState.AddModelError("", "Error al crear usuario: " + apiResponse.message);
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al procesar la creación de la cuenta: " + ex.Message);
                return View();
            }
        }

        private async Task<bool> ValidarReCaptcha(string gRecaptchaResponse)
        {
            var httpClient = new HttpClient();

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", secretKey),
                new KeyValuePair<string, string>("response", gRecaptchaResponse)
            });

            var response = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(responseData);
                return data.success == "true";
            }

            return false;
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