using IdentityModel.Client;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ConsoleClientAD.MSAL
{
    public class ModelBasic
    {
        public string access_token { get; set; }
    }

    /// <summary>
    /// https://docs.microsoft.com/pt-pt/azure/active-directory/develop/scenario-desktop-overview
    /// https://docs.microsoft.com/pt-pt/azure/active-directory/develop/scenario-desktop-app-configuration?tabs=dotnet
    /// https://docs.microsoft.com/pt-pt/azure/active-directory/develop/scenario-desktop-acquire-token-interactive?tabs=dotnet
    /// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Client-credential-flows
    /// </summary>
    class Program
    {

        static string _clientId = "...";
        static string _clientSecret = "...";
        static string _tenantId = "...";
        static string _redirectUri = "http://localhost";

        static void Main(string[] args)
        {
            Client_CredencialMSAL();

            //DeviceFlowSdk();

            //Client_CredencialHttp();
        }

        private static void Client_CredencialMSAL()
        {
            var singletonApp = ConfidentialClientApplicationBuilder.Create(_clientId)
                                   .WithClientSecret(_clientSecret)
                                   .Build();

            var response = singletonApp.AcquireTokenForClient(scopes: new string[] { "api://1ea9629c-d011-40ff-964b-7dfa36c6d9e1/.default" })
                               .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
                               .ExecuteAsync().Result;


            var accessToken = response.AccessToken;

            using (HttpClient httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44316/api/WeatherForecast");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage responseApi = httpClient.SendAsync(request).Result;
                responseApi.EnsureSuccessStatusCode();
                string responseBody = responseApi.Content.ReadAsStringAsync().Result;

                Console.WriteLine("Token:");
                Console.WriteLine(accessToken);

                Console.WriteLine("Response:");
                Console.WriteLine(responseBody);
            }
        }

        private static void DeviceFlowSdk()
        {
            var app = PublicClientApplicationBuilder.Create(_clientId)
                           .WithAuthority(new Uri($"https://login.microsoftonline.com/{_tenantId}"))
                           .WithRedirectUri(_redirectUri)
                           .Build();


            var response = app.AcquireTokenInteractive(new string[] { "api://1ea9629c-d011-40ff-964b-7dfa36c6d9e1/WeatherForecast" })
                        .ExecuteAsync().Result;


            var accessToken = response.AccessToken;

            using (HttpClient httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44316/api/WeatherForecast");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage responseApi = httpClient.SendAsync(request).Result;
                responseApi.EnsureSuccessStatusCode();
                string responseBody = responseApi.Content.ReadAsStringAsync().Result;

                Console.WriteLine("Token:");
                Console.WriteLine(accessToken);

                Console.WriteLine("Response:");
                Console.WriteLine(responseBody);
            }
        }

        private static void Client_CredencialHttp()
        {

            var client = new HttpClient();
            var response = client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = $"https://login.microsoftonline.com/73d21e98-3ff5-4e78-abeb-1d70104eec75/oauth2/v2.0/token",
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Scope = "api://1ea9629c-d011-40ff-964b-7dfa36c6d9e1/.default"

            }).Result;

            var accessToken = response.AccessToken;

            using (HttpClient httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44316/api/WeatherForecast");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage responseApi = httpClient.SendAsync(request).Result;
                responseApi.EnsureSuccessStatusCode();
                string responseBody = responseApi.Content.ReadAsStringAsync().Result;

                Console.WriteLine("Token:");
                Console.WriteLine(accessToken);

                Console.WriteLine("Response:");
                Console.WriteLine(responseBody);
            }

            Console.WriteLine("Hello World!");
        }
    }
}
