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
                            Console.WriteLine("aerolineas: " + aerolineas.Count);
                            // Obtener los datos de consecutivo de aerolíneas de la API o de la base de datos, según corresponda

                            var ultimoPrefijoAero = await ObtenerUltimoPrefijoAeroDesdeAPI();
                            var numeroConsecutivo = await ObtenerNumeroConsecutivoDesdeAPI();

                            Console.WriteLine("Error: " + ultimoPrefijoAero + " y otro error: " + numeroConsecutivo);

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



        public async Task<ActionResult> Bitacora()
        {
            string ConsultaApi = "http://localhost:5056/api/Values/Obtener-Bitacora";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ConsultaApi);

                // Verificar si la solicitud fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta JSON
                    var json = await response.Content.ReadAsStringAsync();

                    try
                    {
                        // Deserializar la respuesta JSON directamente a una lista de objetos Vitacora
                        var vitacoras = JsonConvert.DeserializeObject<List<Vitacora>>(json);


                        // Devolver la vista con los datos
                        return View(vitacoras);
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

        public class ByteArrayToDateTimeConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(DateTime?);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Bytes)
                {
                    var bytes = (byte[])reader.Value;
                    if (bytes.Length == 8) // Timestamp tiene 8 bytes
                    {
                        long timestamp = BitConverter.ToInt64(bytes, 0);
                        return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                    }
                }

                return null;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }



        public async Task<ActionResult> AerolineasPorPais()
        {
            try
            {
                // Llamar al método para obtener los datos de aerolíneas por país desde la API
                var aerolineasPorPais = await GetAerolineasPorPais();

                // Pasar los datos a la vista
                return View(aerolineasPorPais);
            }
            catch (Exception ex)
            {
                // Manejar errores
                return View("Error", $"Error al obtener los datos de aerolíneas por país: {ex.Message}");
            }
        }


        private async Task<List<Aerolineas_Pais>> GetAerolineasPorPais()
        {

            string consultaApi = "http://localhost:5056/api/Values/Obtener-AerolineasPorPais";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(consultaApi);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Aerolineas_Pais>>(json);
                }
                else
                {
                    // Manejar errores de solicitud HTTP
                    throw new HttpRequestException($"Error al realizar la solicitud HTTP: {(int)response.StatusCode} - {response.ReasonPhrase}");
                }
            }

        }
    
    



        //Puertas


        public async Task<ActionResult> Puertas()
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



        
    }
}

