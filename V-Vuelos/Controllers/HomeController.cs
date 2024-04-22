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
        public async Task<ActionResult> Index()
        {

            try
            {
                // Llamar al método para obtener los datos de aerolíneas por país desde la API
                var aerolineasPorPais = await GetPais();

                // Pasar los datos a la vista
                return View(aerolineasPorPais);
            }
            catch (Exception ex)
            {
                // Manejar errores
                return View("Error", $"Error al obtener los datos de aerolíneas por país: {ex.Message}");
            }
           
        }

        private async Task<List<Pai>> GetPais()
        {

            string consultaApi = "http://localhost:5056/api/Values/Obtener-Paises";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(consultaApi);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Pai>>(json);
                }
                else
                {
                    // Manejar errores de solicitud HTTP
                    throw new HttpRequestException($"Error al realizar la solicitud HTTP: {(int)response.StatusCode} - {response.ReasonPhrase}");
                }
            }

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

        public async Task<ActionResult> MetodoPago(int ID)
        {
            try
            {
                string ConsultaApi = $"http://localhost:5056/api/Values/Obtener-Tarjetas/{ID}";

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
                            var Tarjetas = JsonConvert.DeserializeObject<List<Tarjeta>>(json);
                            Console.WriteLine("Tarjetas: " + Tarjetas.Count);

                            ViewBag.Tarjetas = Tarjetas;

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



        public ActionResult LogOut()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");

        }

        public ActionResult IniciarSesionGoogle()
        {
            // Redirigir al usuario a la URL de autorización de Google
            var redirectUri = Url.Action("Callback", "Home", null, Request.Url.Scheme);
            var clientId = "743208489889-85e9oneca0brd1c7lciibnm35n6kaiua.apps.googleusercontent.com";
            var scope = "openid email profile"; // Los alcances que necesites
            var url = $"https://accounts.google.com/o/oauth2/auth?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}";
            return Redirect(url);
        }



        //public async void Callback(string code)
        //{
        //    // Intercambiar el código de autorización por un token de acceso de Google
        //    // Aquí debes hacer una solicitud HTTP a Google para obtener el token de acceso utilizando el código de autorización
        //    // Una vez que obtengas el token de acceso, puedes autenticar al usuario en tu aplicación y redirigirlo a la página deseada
        //    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");
        //    var parameters = new Dictionary<string, string>
        //        {
        //            { "code", code },
        //            { "client_id", "743208489889-85e9oneca0brd1c7lciibnm35n6kaiua.apps.googleusercontent.com" },
        //            { "client_secret", "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" },
        //            { "redirect_uri", Url.Action("Callback", "Home", null, Request.Url.Scheme) },
        //            { "grant_type", "authorization_code" }
        //        };
        //    tokenRequest.Content = new FormUrlEncodedContent(parameters);

        //    var client = new HttpClient();
        //    var tokenResponse = await client.SendAsync(tokenRequest);
        //    var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        //    var tokenData = JsonConvert.DeserializeObject<dynamic>(tokenJson);
        //    var accessToken = tokenData.access_token;

        //    return View("Index");
        //}
    }
}