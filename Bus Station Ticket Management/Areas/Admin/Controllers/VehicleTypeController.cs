using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class VehicleTypeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleTypeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VehicleType
        // [Route("Admin/VehicleType/Index")]
        public async Task<IActionResult> Index(string? sortBy, string? searchString)
        {
            var vehicleTypesList = _context.VehicleTypes.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                vehicleTypesList = vehicleTypesList.Where(vt => vt.Name.Contains(searchString));
            }
            return View(await vehicleTypesList.ToListAsync());
        }

        // GET: VehicleType/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicleType = await _context.VehicleTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicleType == null)
            {
                return NotFound();
            }

            return View(vehicleType);
        }

        public async Task<IActionResult> DetailsPartial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicleType = await _context.VehicleTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicleType == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", vehicleType);
        }

        // GET: VehicleType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VehicleType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,TotalSeats,TotalFloors,TotalColumns,SeatsPerFloor,RowsPerFloor")] VehicleType vehicleType)
        {
            if (VehicleTypeExists(vehicleType.Name))
            {
                ModelState.AddModelError("Name", "This Vehicle Type already exists!");
            }

            if (ModelState.IsValid)
            {
                // Tính tổng số ghế từ SeatsPerFloor (nếu cần)
                if (vehicleType.SeatsPerFloor != null && vehicleType.SeatsPerFloor.Count > 0)
                {
                    vehicleType.TotalSeats = vehicleType.SeatsPerFloor.Sum();
                }

                if (vehicleType.TotalFloors <= 0)
                {
                    ModelState.AddModelError("TotalFloors", "Total Floors must be greater than 0");
                    return View(vehicleType);
                }

                if (vehicleType.SeatsPerFloor == null || vehicleType.SeatsPerFloor.Count == 0)
                {
                    int seatsPerFloor = (int)Math.Ceiling((double)vehicleType.TotalSeats / vehicleType.TotalFloors);
                    vehicleType.SeatsPerFloor = [.. Enumerable.Repeat(seatsPerFloor, vehicleType.TotalFloors)];
                }

                vehicleType.RowsPerFloor = [.. vehicleType.SeatsPerFloor.Select(seats => (int)Math.Ceiling((double)seats / vehicleType.TotalColumns))];

                _context.Add(vehicleType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vehicleType);
        }

        // GET: VehicleType/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicleType = await _context.VehicleTypes.FindAsync(id);
            if (vehicleType == null)
            {
                return NotFound();
            }
            return View(vehicleType);
        }

        // POST: VehicleType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,TotalSeats,TotalRows,TotalColumns,TotalFloors")] VehicleType vehicleType)
        {
            if (id != vehicleType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehicleType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleTypeExists(vehicleType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vehicleType);
        }

        // GET: VehicleType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicleType = await _context.VehicleTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicleType == null)
            {
                return NotFound();
            }

            return View(vehicleType);
        }

        // POST: VehicleType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicleType = await _context.VehicleTypes.FindAsync(id);
            if (vehicleType != null)
            {
                _context.VehicleTypes.Remove(vehicleType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleTypeExists(int id)
        {
            return _context.VehicleTypes.Any(e => e.Id == id);
        }

        private bool VehicleTypeExists(string name)
        {
            return _context.VehicleTypes.Any(e => e.Name == name);
        }
    }
}
