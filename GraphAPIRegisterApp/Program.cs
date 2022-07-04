using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace GraphAPIRegisterApp
{
    public class ModelBasic
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var client_id = "seu clientId";
            var client_secret = "sua secret";
            var tenantId = "Seu id de Diretorio";

            //Obter Token Azure AD Client Credencial Graph
            var clientTokenGraph = new HttpClient();
            var paramsUrlGraph = new Dictionary<string, string>() {

                {"client_id",client_id},
                {"client_secret" , client_secret },
                {"grant_type" , "client_credentials" },
                {"scope" , "https://graph.microsoft.com/.default" }
            };

            var urlGraph = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            var requestrequestGraph = new HttpRequestMessage(HttpMethod.Post, urlGraph)
            {
                Content = new FormUrlEncodedContent(paramsUrlGraph)
            };
            var resGraph = clientTokenGraph.SendAsync(requestrequestGraph).Result;
            var dataGraph = resGraph.Content.ReadAsStringAsync().Result;
            var resultGraph = System.Text.Json.JsonSerializer.Deserialize<ModelBasic>(dataGraph);

            RegisterAppHttp(resultGraph.access_token);


        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/graph/api/application-post-applications?view=graph-rest-beta&tabs=http
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private static void RegisterAppHttp(string accessToken)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"https://graph.microsoft.com/beta/");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");


                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    displayName = "teste APP Ms Graph 02",
                });
                var changePasswordResponse = client.PostAsync("applications", new StringContent(json, Encoding.UTF8, "application/json")).Result;
                var statusCode = changePasswordResponse.StatusCode;
            }

        }
    }
}
