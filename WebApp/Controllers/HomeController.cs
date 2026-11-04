using Microsoft.AspNetCore.Mvc;
using ModelClass.Models;
using Newtonsoft.Json;
using RestSharp;
using System.Diagnostics;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private string JwtToken => HttpContext.Session.GetString("tokenKey") ?? string.Empty;

        private readonly ILogger<HomeController> _logger;
        private readonly string _apiBaseUrl;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _apiBaseUrl = configuration["Api:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("Set Api:BaseUrl in WebApp configuration.");
        }

        public IActionResult Login()
        {
            return View();
        }

        //[Authorize]
        public IActionResult Index()
        {
            var userJson = HttpContext.Session.GetString("User");
            User user = new User();

            if (!string.IsNullOrEmpty(userJson))
            {
                user = JsonConvert.DeserializeObject<User>(userJson);
            }

            return View(user);
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


        public IActionResult LayerControl(int id)
        {
            return PartialView(DefaultLayersContent(id));
        }

        public IActionResult DataEntry()
        {
            return PartialView();
        }

        public IActionResult DataQuery()
        {
            return PartialView();
        }

        public IActionResult LayerOpacity()
        {
            return PartialView();
        }

        public IActionResult CoordinateEntry()
        {
            return PartialView();
        }

        private List<Layers> DefaultLayersContent(int id)
        {
            List<Layers> layers = new List<Layers>();

            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/DefaultLayerSettings?id={id}");

            request.AddHeader("Authorization", "Bearer " + JwtToken);

            //request.AddHeader("Content-Type", "application/json");
            var response = client.Get(request);

            layers = JsonConvert.DeserializeObject<List<Layers>>(response.Content);

            HttpContext.Session.SetString("Layers", JsonConvert.SerializeObject(layers));
            RedirectToAction("LayersSideBar", layers);

            return layers;
        }

    }
}