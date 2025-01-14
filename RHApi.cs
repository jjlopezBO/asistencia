using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Asistencia
{
    public class RecursosHumanosWebApi
    {
        private static RecursosHumanosWebApi _instancia;
        private RecursosHumanosWebApi() { }

        public static RecursosHumanosWebApi Instancia
        {
            get { return _instancia ?? (_instancia = new RecursosHumanosWebApi()); }
        }

        public async Task<object> GetParametroPersonal()
        {
            return SerializadorJson.DeserializeJson<ResponsePersonal>(await UtilesWebApi.Instancia.GetRequestApi(new Uri("https://cndc.salar11.net/api/Colaboradores/Consultar"), HttpMethod.Post, GetHttpRequestPersonal(), await GetTok())).Resultado.OrderBy(x => x.NombreCompleto).ToList();
        }

        public async Task<string> GetTok()
        {
            return SerializadorJson.DeserializeJson<AutorizaConsulta>(await UtilesWebApi.Instancia.GetRequestApi(new Uri("https://cndc.salar11.net/api/Autenticacion/autenticar"), HttpMethod.Post, GetHttpResponseToken(""))).Token;
        }
        internal class AutorizaConsulta
        {
            public string Token { get; set; }
        }

        private string GetHttpResponseToken(string parametro = "")
        {
            return SerializadorJson.SerializeJson(
                    new
                    {
                        IdEmpresa = "123",
                        Cuenta = "api_cndc",
                        Contrasena = "RI#J6ODG"
                    });
        }

        private string GetHttpRequestPersonal()
        {
            return "{\"FechaIngresoInicio\":null,\"FechaIngresoFin\":null,\"CodigoColaborador\":null}";
        }
    }

    class ResponsePersonal
    {
        public List<JsonRegistroPersonal> Resultado { get; set; }
    }

    public class JsonRegistroPersonal
    {
        public string CodigoColaborador { get; set; }
        public string PrimerNombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string NombreCompleto
        {
            get
            {
                return string.Format("{0} {1} {2}", PrimerNombre, ApellidoPaterno, ApellidoMaterno);
            }
        }
    }
}
