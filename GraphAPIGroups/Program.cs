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
            var urlGraphUsers = "https://graph.microsoft.com/v1.0/users/usuario1@wilsonsantosnetgmail.onmicrosoft.com";
            using (HttpClient userClient = new HttpClient())
            {
                var requestUser = new HttpRequestMessage(HttpMethod.Get, urlGraphUsers);
                requestUser.Headers.Authorization = new AuthenticationHeaderValue("Bearer", resultGraph.access_token);
                HttpResponseMessage responseUser = userClient.SendAsync(requestUser).Result;
                responseUser.EnsureSuccessStatusCode();
                var dataGraphUser = responseUser.Content.ReadAsStringAsync().Result;
                resultGraphUsers = System.Text.Json.JsonSerializer.Deserialize<GraphModel>(dataGraphUser);
            }

            //// Chamada GRAph com token Bearer
            //var resultGraphUsers = new GraphModel();
            //var urlGraphUsers = "https://graph.microsoft.com/v1.0/users";
            //using (HttpClient userClient = new HttpClient())
            //{
            //    var requestUser = new HttpRequestMessage(HttpMethod.Get, urlGraphUsers);
            //    requestUser.Headers.Authorization = new AuthenticationHeaderValue("Bearer", resultGraph.access_token);
            //    HttpResponseMessage responseUser = userClient.SendAsync(requestUser).Result;
            //    responseUser.EnsureSuccessStatusCode();
            //    var dataGraphUser = responseUser.Content.ReadAsStringAsync().Result;
            //    resultGraphUsers = System.Text.Json.JsonSerializer.Deserialize<GraphModel>(dataGraphUser);
            //}

            //Criar o grupo
            using (var clientGroup = new HttpClient())
            {
                clientGroup.BaseAddress = new Uri($"https://graph.microsoft.com");
                clientGroup.DefaultRequestHeaders.Add("Authorization", $"Bearer {resultGraph.access_token}");
                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    description = "Teste2",
                    displayName = "Teste2",
                    groupTypes = new List<string> { "Unified", "DynamicMembership" },
                    mailEnabled = "false",
                    mailNickname = "testnickname",
                    membershipRule = "(user.userPrincipalName -notContains \"#EXT#@\") -and (user.userType -ne \"Guest\")",
                    membershipRuleProcessingState = "On",
                    securityEnabled = "true",
                });
                var responseGroup = clientGroup.PostAsync("beta/groups", new StringContent(json, Encoding.UTF8, "application/json")).Result;
                var statusCode = responseGroup.StatusCode;
                var dataMyApi = responseGroup.Content.ReadAsStringAsync().Result;
            }

            //Listar Grupos
            var urlGraphGroups = "https://graph.microsoft.com/v1.0/groups";
            using (HttpClient userClient = new HttpClient())
            {
                var requestUser = new HttpRequestMessage(HttpMethod.Get, urlGraphGroups);
                requestUser.Headers.Authorization = new AuthenticationHeaderValue("Bearer", resultGraph.access_token);
                HttpResponseMessage responseGroups = userClient.SendAsync(requestUser).Result;
                responseGroups.EnsureSuccessStatusCode();
                var dataGraphGruops = responseGroups.Content.ReadAsStringAsync().Result;
                var listGroups = JsonConvert.DeserializeObject<GraphModel>(dataGraphGruops);

                var id = listGroups.value.FirstOrDefault().id;

                //Alterar os grupos
                using (var clientGroup = new HttpClient())
                {
                    clientGroup.BaseAddress = new Uri($"https://graph.microsoft.com");
                    clientGroup.DefaultRequestHeaders.Add("Authorization", $"Bearer {resultGraph.access_token}");
                    var json = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        description = "Teste4",
                        displayName = "Teste4",
                        membershipRule = "(user.userPrincipalName -notContains \"#EXT4#@\") -and (user.userType -ne \"Guest\")",
                    });
                    var responseGroup = clientGroup.PatchAsync($"beta/groups/{id}", new StringContent(json, Encoding.UTF8, "application/json")).Result;
                    var statusCode = responseGroup.StatusCode;
                    var dataMyApi = responseGroup.Content.ReadAsStringAsync().Result;
                }



                // Deletar Grupo
                using (var clientGroup = new HttpClient())
                {
                    clientGroup.BaseAddress = new Uri($"https://graph.microsoft.com");
                    clientGroup.DefaultRequestHeaders.Add("Authorization", $"Bearer {resultGraph.access_token}");
                    var responseGroup = clientGroup.DeleteAsync($"beta/groups/{id}").Result;
                    var statusCode = responseGroup.StatusCode;
                    var dataMyApi = responseGroup.Content.ReadAsStringAsync().Result;
                }

            }



        }
    }
}
