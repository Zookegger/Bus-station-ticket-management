using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bus_Station_Ticket_Management.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee,Admin")]
    [Route("Employee/[Controller]/[Action]")]

    public class HomeController : Controller {
        private readonly ApplicationDbContext _context;
        
        public HomeController (ApplicationDbContext context){
            _context = context;
        }

        public IActionResult Index() {
            return View();
        }
    }
}