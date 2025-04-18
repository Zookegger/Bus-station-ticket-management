using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace Bus_Station_Ticket_Management.Controllers
{
    [Route("[controller]/[action]")]
    public class LocationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Search(string? term) {
            var locations = _context.Locations.AsQueryable();
            if (term != null) {
                locations = locations.Where(x => x.Name.Contains(term) || x.Address.Contains(term));
            }

            return Json(new { success = true, locationList = locations});
        }

        [HttpGet]
        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }
    }
}
