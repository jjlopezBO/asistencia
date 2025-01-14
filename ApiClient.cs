using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;

namespace Asistencia
{

    public class ApiClient
    {
        private readonly HttpClient _client;

        public ApiClient()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://cndc.salar11.net/api/")
            };
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<string> AutenticarAsync(int idEmpresa, string cuenta, string contrasena)
        {
            try
            {
                var request = new
                {
                    IdEmpresa = idEmpresa,
                    Cuenta = cuenta,
                    Contrasena = contrasena
                };

                string jsonRequest =System.Text.Json.JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("Autenticacion/Autenticar", content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var authResponse = System.Text.Json.JsonSerializer.Deserialize<AutenticacionResponse>(jsonResponse);

                    if (authResponse.Exito)
                    {
                        return authResponse.Token;
                    }
                    else
                    {
                        Console.WriteLine("Error en la autenticación:");
                        foreach (var mensaje in authResponse.Mensajes)
                        {
                            Console.WriteLine(mensaje);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error en la solicitud de autenticación: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción en AutenticarAsync: {ex.Message}");
            }

            return null;
        }

        public async Task<string> EjecutarConsultaParametrizadaAsync(string token, ConsultaParametrizadaRequest request)
        {
            try
            {
                string jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _client.PostAsync("ConsultasParametrizadas/EjecutarConsultaParametrizada", content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return jsonResponse;
                }
                else
                {
                    Console.WriteLine($"Error en la solicitud de consulta parametrizada: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción en EjecutarConsultaParametrizadaAsync: {ex.Message}");
            }

            return null;
        }

        public async Task<ConsultarHorasTrabajadasResponse> ConsultarHorasTrabajadasAsync(string token, string regional, string periodo)
        {
            try
            {
                var request = new
                {
                    Regional = regional,
                    Periodo = periodo
                };

                string jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _client.PostAsync("Asistencia/ConsultarHorasTrabajadas", content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var consultarResponse = JsonSerializer.Deserialize<ConsultarHorasTrabajadasResponse>(jsonResponse);
                    return consultarResponse;
                }
                else
                {
                    Console.WriteLine($"Error en la solicitud de consulta de horas trabajadas: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción en ConsultarHorasTrabajadasAsync: {ex.Message}");
            }

            return null;
        }




    }
    public class AutenticacionResponse
    {
        public bool Exito { get; set; }
        public List<string> Mensajes { get; set; }
        public string Token { get; set; }
        public string NombreCompleto { get; set; }
        public int TipoError { get; set; }
        public bool UsuarioInterno { get; set; }
        public List<Menu> Menus { get; set; }
        public string Version { get; set; }
    }
    public class Menu
    {
        public string Codigo { get; set; }
        public string Etiqueta { get; set; }
    }

    public class ConsultaParametrizadaRequest
    {
        public string CodigoConsulta { get; set; }
        public string Modo { get; set; }
        public List<Parametro> Parametros { get; set; }
    }
    public class Parametro
    {
        public string Nombre { get; set; }
        public string Valor { get; set; }
        public int Tipo { get; set; }
    }

}
