using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class EntryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BuildingEntry()
        {
            return PartialView();
        }
        public IActionResult CityEntry()
        {
            return PartialView();
        }
        public IActionResult CountyEntry()
        {
            return PartialView();
        }
        public IActionResult NeighborhoodEntry()
        {
            return PartialView();
        }
        public IActionResult StraightEntry()
        {
            return PartialView();
        }
        public IActionResult StreetEntry()
        {
            return PartialView();
        }
        public IActionResult SubscriberEntry()
        {
            return PartialView();
        }
        public IActionResult TransformerEntry()
        {
            return PartialView();
        }
    }
}
