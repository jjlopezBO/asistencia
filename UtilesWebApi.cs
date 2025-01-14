using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Asistencia
{
    public class UtilesWebApi
    {
        private static UtilesWebApi _instancia;
        private UtilesWebApi() { }

        public static UtilesWebApi Instancia
        {
            get { return _instancia ?? (_instancia = new UtilesWebApi()); }
        }

        readonly string _ContentTypeJson = "application/json";

        public async Task<string> GetRequestApi(Uri direccion, HttpMethod metodo, string parametro, string token = "")
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(metodo, direccion))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(_ContentTypeJson));
                    request.Headers.Add("Cache-Control", "no-cache");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.Timeout = TimeSpan.FromSeconds(300);
                    request.Content = new StringContent(parametro, Encoding.UTF8, _ContentTypeJson);

                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string content = await response.Content.ReadAsStringAsync();
                            return content;
                        }
                        else
                        {
                            return string.Format("Error: {0}", response.StatusCode);
                        }
                    }
                }
            }
        }
        /*
        public RootObject InvokeAPI()
        {
            RootObject apiresponse = new RootObject();
            string result = string.Empty;
            HttpClientFactory clientFactory = new HttpClientFactory();
            var client = clientFactory.CreateClient();
            HttpResponseMessage response = client.GetAsync("api/aes").Result;
            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
                apiresponse = JsonConvert.DeserializeObject<RootObject>(result);
            }
            return apiresponse;
        }*/
    }
}
