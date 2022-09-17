using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication8.Models;

namespace WebApplication8.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration Configuration;

        public HomeController(ILogger<HomeController> logger,IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
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

        [HttpGet("/get/the/authorization/code")]
        public IActionResult GetTheCode() {

            string Authorization_Endpoint = Configuration["OAuth:Authorization_Endpoint"];
            string Response_Type = "code";
            string Client_Id = Configuration["OAuth:Client_Id"];
            string Redirect_Uri = Configuration["OAuth:Redirect_Uri"];
            string Scope = Configuration["OAuth:Scope"];
            const string State = "Estado";

            string URL = $"{Authorization_Endpoint}?" +
                $"response_type={Response_Type}&" +
                $"client_id={Client_Id}&" +
                $"redirect_uri={Redirect_Uri}&" +
                $"scope={Scope}&state={State}";

            return Redirect(URL);

        }

        [HttpGet("/authentication/login-callback")]
        public IActionResult LoginCallBack([FromQuery] string code, string state) {

            return View((code, state));
        }

        [HttpGet("/exchange/token")]
        public async Task <IActionResult>  ExchangeLoginCallBack(string code, string state)
        {

            const string Grant_Type = "authorization_code";
            string Token_Endpoint = Configuration["OAuth:Token_Endpoint"];
            string Redirect_Uri = Configuration["OAuth:Redirect_Uri"];
            string Client_Id = Configuration["OAuth:Client_Id"];
            string Client_Secret = Configuration["OAuth:Client_Secret"];
            string Scope = Configuration["OAuth:Scope"];

            Dictionary<string, string> BodyData = new Dictionary<string, string>
            {
                { "grant_type",Grant_Type},
                { "code",code},
                {"redirect_uri",Redirect_Uri },
                { "client_id",Client_Id},
                { "client_secret",Client_Secret},
                { "scope",Scope}
            };

            HttpClient httpClient = new HttpClient();
            
            var Body = new FormUrlEncodedContent(BodyData);

            var Response = await httpClient.PostAsync(Token_Endpoint, Body);

            var Status = $"{(int)Response.StatusCode}{Response.ReasonPhrase}";

            var JsonContent = await Response.Content.ReadFromJsonAsync<JsonElement>();

            var PrettyPrintJsonContent = JsonSerializer.Serialize(JsonContent, new JsonSerializerOptions { WriteIndented = true });

            return View((Status, PrettyPrintJsonContent, Response.IsSuccessStatusCode));

        }

    }
}
