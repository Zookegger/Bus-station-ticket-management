using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DriverLicensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DriverLicensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/DriverLicenses
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.DriverLicenses.Include(d => d.Driver);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/DriverLicenses/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driverLicense = await _context.DriverLicenses
                .Include(d => d.Driver)
                .FirstOrDefaultAsync(m => m.DriverId == id);
            if (driverLicense == null)
            {
                return NotFound();
            }

            return View(driverLicense);
        }

        // GET: Admin/DriverLicenses/Create
        public IActionResult Create()
        {
            ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "Id");
            return View();
        }

        // POST: Admin/DriverLicenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DriverId,LicenseId,LicenseClass,LicenseIssueDate,LicenseExpirationDate,LicenseIssuePlace")] DriverLicense driverLicense)
        {
            if (ModelState.IsValid)
            {
                _context.Add(driverLicense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "Id", driverLicense.DriverId);
            return View(driverLicense);
        }

        // GET: Admin/DriverLicenses/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driverLicense = await _context.DriverLicenses.FindAsync(id);
            if (driverLicense == null)
            {
                return NotFound();
            }
            ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "Id", driverLicense.DriverId);
            return View(driverLicense);
        }

        // POST: Admin/DriverLicenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("DriverId,LicenseId,LicenseClass,LicenseIssueDate,LicenseExpirationDate,LicenseIssuePlace")] DriverLicense driverLicense)
        {
            if (id != driverLicense.DriverId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(driverLicense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriverLicenseExists(driverLicense.DriverId))
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
            ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "Id", driverLicense.DriverId);
            return View(driverLicense);
        }

        // GET: Admin/DriverLicenses/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driverLicense = await _context.DriverLicenses
                .Include(d => d.Driver)
                .FirstOrDefaultAsync(m => m.DriverId == id);
            if (driverLicense == null)
            {
                return NotFound();
            }

            return View(driverLicense);
        }

        public async Task<IActionResult> DetailsPartial(string driverId, string licenseId)
        {
            var license = await _context.DriverLicenses
                .Include(l => l.Driver)
                .FirstOrDefaultAsync(l => l.DriverId == driverId && l.LicenseId == licenseId);
            if (license == null) return NotFound();
            return PartialView("_DetailsPartial", license);
        }



        // POST: Admin/DriverLicenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var driverLicense = await _context.DriverLicenses.FindAsync(id);
            if (driverLicense != null)
            {
                _context.DriverLicenses.Remove(driverLicense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DriverLicenseExists(string id)
        {
            return _context.DriverLicenses.Any(e => e.DriverId == id);
        }
    }
}
