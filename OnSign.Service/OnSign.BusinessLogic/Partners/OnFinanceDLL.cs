using Newtonsoft.Json;
using OnSign.BusinessObject.Partners;
using OnSign.BusinessObject.Forms;
using OnSign.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnSign.BusinessLogic.Partners
{
    public class OnFinanceDLL
    {
        private HttpClient _client = null;
        public OnFinanceDLL()
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri(ConfigHelper.OnFinanceHost)
            };
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetStringAsync(string resource)
        {
            using (var response = await _client.GetAsync(resource))
            {
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    return responseData;
                }
                response.EnsureSuccessStatusCode();
                return default(string);
            }
        }

        public async Task<string> PostStringAsync(string resource, string body)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["AccessToken"]);
            var stringContent = new StringContent(body, Encoding.UTF8, "application/json");
            using (var response = await _client.PostAsync(resource, stringContent))
            {
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    return responseData;
                }
                response.EnsureSuccessStatusCode();
                return default(string);
            }
        }

        public void GetAccessToken(string taxcode)
        {
            new Thread(async () =>
            {
                try
                {
                    string api = $"{ConfigHelper.OnFinanceAPIGetToken}?mst={taxcode}";
                    var response = await this.GetStringAsync(api);
                    if (!string.IsNullOrEmpty(response))
                    {
                        ConfigurationManager.AppSettings["AccessToken"] = JsonConvert.DeserializeObject<TokenOnFinanceBO>(response, new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        }).Token;
                    }
                }
                catch (Exception)
                {
                }

            }).Start();
        }
    }
}
