using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ConsoleApp1
{
    

    public class ModelBasic
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }


    public class GraphModel
    {

        public class GraphItem
        {

            public string displayName { get; set; }
            public string id { get; set; }

        }
        public GraphItem[] value { get; set; }

    }

    /// <summary>
    /// https://docs.microsoft.com/pt-br/graph/api/resources/conditionalaccesspolicy?view=graph-rest-1.0
    /// https://docs.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=CS#client-credentials-provider
    /// </summary>
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

            // Chamada GRAph com token Bearer
            var resultGraphUsers = new GraphModel();
            var urlGraphUsers = "https://graph.microsoft.com/v1.0/users";
            using (HttpClient userClient = new HttpClient())
            {
                var requestUser = new HttpRequestMessage(HttpMethod.Get, urlGraphUsers);
                requestUser.Headers.Authorization = new AuthenticationHeaderValue("Bearer", resultGraph.access_token);
                HttpResponseMessage responseUser = userClient.SendAsync(requestUser).Result;
                responseUser.EnsureSuccessStatusCode();
                var dataGraphUser = responseUser.Content.ReadAsStringAsync().Result;
                resultGraphUsers = System.Text.Json.JsonSerializer.Deserialize<GraphModel>(dataGraphUser);
            }

            //Listar Politicas
            var urlGraphPolicies = "https://graph.microsoft.com/v1.0/identity/conditionalAccess/policies";
            using (HttpClient userClient = new HttpClient())
            {
                var requestUser = new HttpRequestMessage(HttpMethod.Get, urlGraphPolicies);
                requestUser.Headers.Authorization = new AuthenticationHeaderValue("Bearer", resultGraph.access_token);
                HttpResponseMessage responsePolicy = userClient.SendAsync(requestUser).Result;
                responsePolicy.EnsureSuccessStatusCode();
                var dataGraphGruops = responsePolicy.Content.ReadAsStringAsync().Result;
                var listData = JsonConvert.DeserializeObject<GraphModel>(dataGraphGruops);

                var id = listData.value.FirstOrDefault().id;

            }

            //Criar uma Politica
            using (var clientPolicy = new HttpClient())
            {
                clientPolicy.BaseAddress = new Uri($"https://graph.microsoft.com");
                clientPolicy.DefaultRequestHeaders.Add("Authorization", $"Bearer {resultGraph.access_token}");
                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    displayName = "Access to EXO requires MFA 2",
                    state = "enabled",
                    conditions = new
                    {
                        clientAppTypes = new string[] { "mobileAppsAndDesktopClients", "browser" },
                        applications = new
                        {
                            includeApplications = new string[] { "ce076710-03a8-42c0-9550-f7157433dbb1" }
                        },
                        users = new
                        {
                            includeGroups = new string[] { "95016f14-547b-4cd5-bee1-97af546e207a" }
                        },
                        locations = new
                        {
                            includeLocations = new string[] { "All" },
                            excludeLocations = new string[] { "AllTrusted" }
                        }
                    },
                    grantControls = new
                    {
                        Operator = "OR",
                        builtInControls = new string[] { "mfa" }
                    }
                });

                var responsePolicy = clientPolicy.PostAsync("v1.0/identity/conditionalAccess/policies", new StringContent(json, Encoding.UTF8, "application/json")).Result;
                var statusCode = responsePolicy.StatusCode;
                var dataMyApi = responsePolicy.Content.ReadAsStringAsync().Result;

            }


          

        }
    }
}
