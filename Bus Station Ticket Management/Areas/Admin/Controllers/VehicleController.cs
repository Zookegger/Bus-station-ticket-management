﻿using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehicleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vehicle
        [HttpGet]
        [Route("Admin/Vehicle/Index")]
        public async Task<IActionResult> Index(string? searchString, int? page, string? sortBy, string? filterByStatus, string? filterByType)
        {
            int pageSize = 20;
            int pageNumber = page ?? 1;
            var now = DateTime.Now;

            var vehicles = _context.Vehicles.Include(v => v.VehicleType).AsQueryable();

            // Apply search
            if (!string.IsNullOrEmpty(searchString))
            {
                vehicles = vehicles.Where(v =>
                    v.Name.Contains(searchString) ||
                    v.LicensePlate.Contains(searchString) ||
                    v.VehicleType.Name.Contains(searchString)
                );
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(filterByStatus) && filterByStatus != "All")
            {
                vehicles = vehicles.Where(v => v.Status == filterByStatus);
            }

            // Apply vehicle type filter
            if (!string.IsNullOrEmpty(filterByType) && filterByType != "All")
            {
                if (int.TryParse(filterByType, out int typeId))
                {
                    vehicles = vehicles.Where(v => v.VehicleTypeId == typeId);
                }
            }

            // Update vehicle statuses based on trips
            var vehiclesWithTrips = await _context.Trips.Include(t => t.Vehicle).ToListAsync();
            foreach (var trip in vehiclesWithTrips)
            {
                if (trip.DepartureTime <= now && trip.ArrivalTime > now)
                {
                    trip.Vehicle.Status = "In-Progress";
                }
                else if (trip.ArrivalTime <= now)
                {
                    trip.Vehicle.Status = "Stand-By";
                }
            }
            await _context.SaveChangesAsync();

            // Sorting
            switch (sortBy)
            {
                case "name_asc": vehicles = vehicles.OrderBy(v => v.Name); break;
                case "name_desc": vehicles = vehicles.OrderByDescending(v => v.Name); break;
                case "licenseplate_asc": vehicles = vehicles.OrderBy(v => v.LicensePlate); break;
                case "licenseplate_desc": vehicles = vehicles.OrderByDescending(v => v.LicensePlate); break;
                case "type_asc": vehicles = vehicles.OrderBy(v => v.VehicleType.Name); break;
                case "type_desc": vehicles = vehicles.OrderByDescending(v => v.VehicleType.Name); break;
                default: vehicles = vehicles.OrderBy(v => v.Name); break;
            }

            // Pass values back to view
            ViewBag.SortBy = sortBy;
            ViewBag.SearchString = searchString;
            ViewBag.FilterByStatus = filterByStatus;
            ViewBag.FilterByType = filterByType;
            ViewBag.VehicleTypes = new SelectList(_context.VehicleTypes, "Id", "Name");

            var vehicleList = await vehicles.ToListAsync();
            return View(vehicleList.ToPagedList(pageNumber, pageSize));
        }

        // GET: Vehicle/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // GET: Vehicle/Create
        public IActionResult Create()
        {
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name");
            return View();
        }

        // POST: Vehicle/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,AcquiredDate,LicensePlate,Status,VehicleTypeId")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
            return View(vehicle);
        }

        // GET: Vehicle/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
            return View(vehicle);
        }

        // POST: Vehicle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,AcquiredDate,LicensePlate,Status,VehicleTypeId")] Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    vehicle.LastUpdated = DateTime.Now;
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.Id))
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
            ViewData["VehicleTypeId"] = new SelectList(_context.VehicleTypes, "Id", "Name", vehicle.VehicleTypeId);
            return View(vehicle);
        }

        // GET: Vehicle/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
    }
}
