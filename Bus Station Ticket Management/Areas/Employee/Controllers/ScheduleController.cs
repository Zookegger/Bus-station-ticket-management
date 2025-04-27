using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Bus_Station_Ticket_Management.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee,Admin")]
    [Route("Employee/[Controller]/[Action]")]
    
    public class ScheduleController : Controller
    {
        // Sample schedule items
        private static List<ScheduleItem> scheduleItems = new List<ScheduleItem>
        {
            new ScheduleItem { Day = "Monday", TimeSlot = "08:00 - 09:00", Event = "Math Class" },
            new ScheduleItem { Day = "Tuesday", TimeSlot = "09:00 - 10:00", Event = "Physics Class" },
            new ScheduleItem { Day = "Wednesday", TimeSlot = "10:00 - 11:00", Event = "Chemistry Class" },
        };

        // GET: Schedule
        public IActionResult Index()
        {
            return View();
        }

        // GET: Schedule/Events
        public JsonResult GetEvents()
        {
            var events = scheduleItems.Select(item => new
            {
                title = item.Event,
                start = ConvertToDate(item.Day, item.TimeSlot), // Convert to start datetime
                end = ConvertToEndDate(item.Day, item.TimeSlot), // Convert to end datetime
            }).ToList();

            return Json(events);
        }

        // Helper method to convert time and day to datetime format
        private string ConvertToDate(string day, string timeSlot)
        {
            var timeParts = timeSlot.Split(" - ");
            var startTime = timeParts[0];
            var dayOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>()
                .FirstOrDefault(d => d.ToString() == day);

            var startDateTime = DateTime.Today.AddDays((int)dayOfWeek - (int)DateTime.Now.DayOfWeek)
                .Add(TimeSpan.Parse(startTime));

            return startDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        // Helper method to get end time based on time slot
        private string ConvertToEndDate(string day, string timeSlot)
        {
            var timeParts = timeSlot.Split(" - ");
            var startTime = timeParts[0];
            var endTime = timeParts[1];

            var dayOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>()
                .FirstOrDefault(d => d.ToString() == day);

            var endDateTime = DateTime.Today.AddDays((int)dayOfWeek - (int)DateTime.Now.DayOfWeek)
                .Add(TimeSpan.Parse(startTime))
                .Add(TimeSpan.Parse(endTime).Subtract(TimeSpan.Parse(startTime))); // Add duration

            return endDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
        }
    }
}