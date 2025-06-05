using Microsoft.AspNetCore.Mvc;
using PBL3.Models;

namespace PBL3.Controllers
{
    public class SearchController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult SearchByLocation(string location)
        {
            return View();
        }
    }
}