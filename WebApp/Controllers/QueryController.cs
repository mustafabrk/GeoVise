using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class QueryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BuildingQuery()
        {
            return PartialView();
        }
        public IActionResult CityQuery()
        {
            return PartialView();
        }
        public IActionResult CountyQuery()
        {
            return PartialView();
        }
        public IActionResult NeighborhoodQuery()
        {
            return PartialView();
        }
        public IActionResult StraightQuery()
        {
            return PartialView();
        }
        public IActionResult StreetQuery()
        {
            return PartialView();
        }
        public IActionResult SubscriberQuery()
        {
            return PartialView();
        }
        public IActionResult TransformerQuery()
        {
            return PartialView();
        }
    }
}
