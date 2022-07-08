using ClientAD.Models.MSAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClientAD.Controllers.MSAL
{

    public class ModelBasic
    {
        public string access_token { get; set; }
    }
    public class GraphUser
    {
        public string displayName { get; set; }
        public string id { get; set; }

    }
    public class GraphModel
    {

       
        public GraphUser[] value { get; set; }

    }

    [Authorize()]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenAcquisition _tokenAcquisition;
        public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory, IConfiguration config, ITokenAcquisition tokenAcquisition)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _configuration = config;
            _tokenAcquisition = tokenAcquisition;
        }





        public async Task<IActionResult> Index()
        {
            try
            {

                
                var scope = _configuration["Roles:Scope"].Split(" ");
                var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scope);
                //var accessToken2 = await _tokenAcquisition.GetAccessTokenForUserAsync(new string[] { "https://graph.microsoft.com/User.Read" });
                //var accessToken2 = await _tokenAcquisition.GetAccessTokenForUserAsync(new string[] { "https://graph.microsoft.com/User.Read", "https://graph.microsoft.com/User.Read.All", "https://graph.microsoft.com/Directory.AccessAsUser.All" });
                //var accessToken3 = await _tokenAcquisition.GetAccessTokenForUserAsync(new string[] { "api://aa62aba6-1910-4c38-809b-5d2518e17485/AccessAPI" });

                //var responseA = await BackAD(accessToken, "https://localhost:44316");
                //var responseB = await BackAD(accessToken, "https://localhost:44351/");



                //var email = User.Claims.Where(_ => _.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Select(_ => _.Value).SingleOrDefault();

                //Chamada Graph com token Bearer para user logado
                //var resultGraphUser = new GraphUser();
                //var urlGraphUser = $"https://graph.microsoft.com/v1.0/me";
                //using (HttpClient userClient = new HttpClient())
                //{
                //    var requestUser = new HttpRequestMessage(HttpMethod.Get, urlGraphUser);
                //    requestUser.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken2);
                //    HttpResponseMessage responseUser = userClient.SendAsync(requestUser).Result;
                //    responseUser.EnsureSuccessStatusCode();
                //    var dataGraphUser = responseUser.Content.ReadAsStringAsync().Result;
                //    resultGraphUser = System.Text.Json.JsonSerializer.Deserialize<GraphUser>(dataGraphUser);

                //}



                //var result = Getusers(accessToken2);
                //var usuario1 = result.value[4];

                //ChangePasswordProfileWithHttpClient(accessToken2, usuario1.id);
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                var scope = _configuration["Roles:Scope"].Split(" ");
                _tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeader(scope, ex.MsalUiRequiredException);
                return View();
            }
            catch (MsalUiRequiredException ex)
            {
                var scope = _configuration["Roles:Scope"].Split(" ");
                _tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeader(scope, ex);
                return View();
            }
            catch (Exception ex)
            {
                //return Logout();
                throw ex;
            }


            ViewData["Name"] = User.Claims.Where(_ => _.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname").FirstOrDefault().Value;
            return View();
        }

        private async Task<HttpResponseMessage> BackAD(string accessToken, string API)
        {
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri(API);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync($"api/WeatherForecast");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JArray.Parse(responseContent);

            }

            return response;
        }

        public IActionResult Logout()
        {
            var api = new Uri(_configuration["Roles:Scope"]);
            var urllogout = $"https://login.microsoftonline.com/common/oauth2/v2.0/logout?post_logout_redirect_uri={api.AbsoluteUri}signin-oidc";
            return Redirect(urllogout);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
