using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace GraphAPIPassword
{
    public class GraphModel
    {

        public class GraphUser
        {

            public string displayName { get; set; }
            public string id { get; set; }

        }
        public GraphUser[] value { get; set; }

    }
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

            var result = Getusers(resultGraph.access_token);
            var usuario1 = result.value[4];

            ChangePasswordProfileWithHttpClient(resultGraph.access_token, usuario1.id);

        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/graph/api/user-list?view=graph-rest-1.0&tabs=http
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private static GraphModel Getusers(string accessToken)
        {
            var result = new GraphModel();
            var url = "https://graph.microsoft.com/v1.0/users";
            using (HttpClient userClient = new HttpClient())
            {
                var requestUser = new HttpRequestMessage(HttpMethod.Get, url);
                requestUser.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage responseUser = userClient.SendAsync(requestUser).Result;
                responseUser.EnsureSuccessStatusCode();
                var data = responseUser.Content.ReadAsStringAsync().Result;
                result = System.Text.Json.JsonSerializer.Deserialize<GraphModel>(data);

            }

            return result;
        }


        /// <summary>
        /// https://docs.microsoft.com/en-us/previous-versions/azure/ad/graph/api/functions-and-actions#changePassword
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="id"></param>
        private static void ChangePasswordFunction(string accessToken, string id)
        {
            var passwordActual = "";
            var passwordChange = "";

            using (var changePasswordClient = new HttpClient())
            {
                changePasswordClient.BaseAddress = new Uri($"https://graph.windows.net/users/{id}");
                changePasswordClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");


                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    currentPassword = passwordActual,
                    newPassword = passwordChange
                });
                var changePasswordResponse = changePasswordClient.PostAsync("changePassword?api-version-1.6", new StringContent(json, Encoding.UTF8, "application/json")).Result;
                var statusCode = changePasswordResponse.StatusCode;
            }
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/graph/api/user-update?view=graph-rest-1.0&tabs=http#example-3-update-the-passwordprofile-of-a-user-to-reset-their-password
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="id"></param>
        private static void ChangePasswordProfileWithHttpClient(string accessToken, string id)
        {
            var passwordChange = "p@$$w0rd8";

            var urlChangePassword = $"https://graph.microsoft.com/v1.0/users/{id}";
            using (HttpClient changePasswordClient = new HttpClient())
            {

                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    accountEnabled = true,
                    passwordProfile = new
                    {
                        forceChangePasswordNextSignIn = false,
                        password = passwordChange
                    }
                });
                var requestChangePassword = new HttpRequestMessage(HttpMethod.Patch, urlChangePassword);
                requestChangePassword.Content = new StringContent(json, Encoding.UTF8, "application/json");
                requestChangePassword.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var changePasswordResponseApi = changePasswordClient.SendAsync(requestChangePassword).Result;
                changePasswordResponseApi.EnsureSuccessStatusCode();
            }
        }
    }
}
