using IdentityModel.Client;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace graph_api_samples
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
    class Program
    {


        const string clientId = "5a6e7523-dfa0-44d4-8ed7-1127cbb573ed";
        const string clientSecret = "7RDxT1ZWW.a-cj795~LhLbeN_A89NX_W-Q";
        const string tenant = "seedazb2c.onmicrosoft.com";
        const string redirectUri = "http://localhost:1234";
        const string passwordActual = "p@$$w0rd4";
        const string passwordChange = "p@$$w0rd5";


        /// <summary>
        /// https://docs.microsoft.com/pt-br/graph/use-the-api
        /// https://docs.microsoft.com/pt-br/graph/auth-v2-user
        /// https://docs.microsoft.com/pt-br/graph/auth-v2-service
        /// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/System-Browser-on-.Net-Core

        /// https://developer.microsoft.com/en-us/graph/graph-explorer
        /// https://ngrok.com/download
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //CallGraphAPIHttpClient();
            CallMeGraphAPISDK();
            Console.Read();
        }

        private static void CallMeGraphAPISDK()
        {

            var app = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenant}"))
                .WithRedirectUri(redirectUri)
                .Build();

            var authenticationProvider = new InteractiveAuthenticationProvider(app);

            var graphServiceClient = new GraphServiceClient(authenticationProvider);

            var me = graphServiceClient.Me.Request().GetAsync().Result;
            Console.WriteLine($"{me.DisplayName}");


            graphServiceClient.Me.ChangePassword(passwordActual, passwordChange).Request().PostAsync().Wait();


            Console.Read();
        }

        private static void CallGraphAPIHttpClient()
        {
            ///Obtem token para a aplicação Fluxo Client Credencial
            var accessToken = GetAccessTokeToApplicationWithHttpClient();
            Console.WriteLine("Token:");
            Console.WriteLine(accessToken);

            ///Lista usuários do Diretório
            var result = Getusers(accessToken);
            Console.WriteLine("Response:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(result));

            //var id = result.value[0].id;
            var id = result.value[2].id;
            //Tenta Realizar operações de Trocar A senha
            //ChangePasswordFunction(accessToken, id);
            ChangePasswordProfileWithHttpClient(accessToken, id);
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
        private static void ChangePasswordProfileWithHttpClient(string accessToken,string id)
        {
            var urlChangePassword = $"https://graph.microsoft.com/v1.0/users/{id}";
            using (HttpClient changePasswordClient = new HttpClient())
            {

                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    passwordProfile = new
                    {
                        forceChangePasswordNextSignIn = false,
                        password = passwordChange
                    }
                });
                var requestChangePassword = new HttpRequestMessage(HttpMethod.Patch, urlChangePassword);
                requestChangePassword.Content = new StringContent(json, Encoding.UTF8, "application/json");
                requestChangePassword.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage changePasswordResponseApi = changePasswordClient.SendAsync(requestChangePassword).Result;
                changePasswordResponseApi.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// https://docs.microsoft.com/pt-br/graph/auth-v2-service
        /// </summary>
        /// <returns></returns>
        private static string  GetAccessTokeToApplicationWithHttpClient()
        {
            var client = new HttpClient();
            var response = client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token",
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "https://graph.microsoft.com/.default"

            }).Result;
            return response.AccessToken;
        }
    }
}
