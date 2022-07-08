using IdentityModel.Client;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

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

        static string client_id;
        static string client_secret;
        static string tenantId;
        static string redirectUri;


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

            client_id = "53ebfdf3-c8ec-40c5-b922-db3fc7c72bb6";
            client_secret = "YLW8Q~-g~NBEE5JvDEm8sc~J4T8oVW7sOqzoTcY3";
            tenantId = "779811d8-4753-4c34-baeb-6b53957d52e3";

            redirectUri = "http://localhost";


            //CallGraphAPIHttpClient();
            CallMeGraphAPISDKPrivateClient();
            //CallMeGraphAPISDKPublicClient();
            //Console.Read();
        }

        /// <summary>
        /// https://docs.microsoft.com/pt-br/azure/active-directory/develop/msal-net-initializing-client-applications
        /// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/System-Browser-on-.Net-Core
        /// </summary>
        private static void CallMeGraphAPISDKPublicClient()
        {

            var app = PublicClientApplicationBuilder.Create(client_id)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                .WithRedirectUri(redirectUri)
                .Build();

            var authenticationProvider = new InteractiveAuthenticationProvider(app, new string[] { "https://graph.microsoft.com/User.Read " });

            var graphServiceClient = new GraphServiceClient(authenticationProvider);

            var me = graphServiceClient.Me.Request().GetAsync().Result;
            Console.WriteLine($"{me.DisplayName}");


            graphServiceClient.Communications.Request();

            

            Console.Read();
        }


        private static void CallMeGraphAPISDKPrivateClient()
        {
            var scopes = new string[] { "https://graph.microsoft.com/.default" };

            var confidentialClient = ConfidentialClientApplicationBuilder
                .Create(client_id)
                .WithTenantId(tenantId)
                .WithClientSecret(client_secret)
                .Build();

            GraphServiceClient graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) => {

                    // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                    var authResult = await confidentialClient
                        .AcquireTokenForClient(scopes)
                        .ExecuteAsync();

                    // Add the access token in the Authorization header of the API request.
                    requestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                })
            );


            var users = graphServiceClient.Users.Request().GetAsync().Result;


            Console.WriteLine($"{users.FirstOrDefault().DisplayName}");
            graphServiceClient.Communications.Request();

        }



        private static void CallGraphAPIHttpClient()
        {
            ///Obtem token para a aplicação Fluxo Client Credencial
            var accessToken = GetAccessTokeToApplicationWithHttpClientAzureAD();
            Console.WriteLine("Token:");
            Console.WriteLine(accessToken);

            ///Lista usuários do Diretório
            var result = Getusers(accessToken);
            Console.WriteLine("Response:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(result));

            var id = result.value[2].id;
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
        /// https://docs.microsoft.com/pt-br/graph/auth-v2-service
        /// </summary>
        /// <returns></returns>
        private static string GetAccessTokeToApplicationWithHttpClientAzureAD()
        {
            var client = new HttpClient();
            var response = client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
                ClientId = client_id,
                ClientSecret = client_secret,
                Scope = "https://graph.microsoft.com/.default"

            }).Result;
            return response.AccessToken;
        }
    }
}
