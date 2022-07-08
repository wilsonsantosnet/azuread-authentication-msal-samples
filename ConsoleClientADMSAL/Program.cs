using IdentityModel.Client;
using System;
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
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var client = new HttpClient();
            var response = client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = $"https://login.microsoftonline.com/73d21e98-3ff5-4e78-abeb-1d70104eec75/oauth2/v2.0/token",
                ClientId = "...",
                ClientSecret = "..",
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
