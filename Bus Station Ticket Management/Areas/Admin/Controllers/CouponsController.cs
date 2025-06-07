using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]

    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CouponsController> _logger;

        public CouponsController(ApplicationDbContext context, ILogger<CouponsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Coupons
        public async Task<IActionResult> Index()
        {
            try
            {
                var coupons = await _context.Coupons.ToListAsync();
                return View(coupons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting index");
                return View();
            }
        }

        // GET: Admin/Coupons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (coupon == null)
                {
                    return NotFound();
                }

                return View(coupon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting details");
                return NotFound();
            }
        }

        public async Task<IActionResult> DetailsPartial(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (coupon == null)
                {
                    return NotFound();
                }

                return PartialView("_DetailsPartial", coupon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting details partial");
                return NotFound();
            }
        }

        // GET: Admin/Coupons/Create
        public IActionResult Create()
        {
            try
            {
                ViewBag.DiscountType = new SelectList(
                    Enum.GetValues(typeof(DiscountType))
                    .Cast<DiscountType>()
                    .Select(dt => new
                    {
                        Value = dt,
                        Text = dt.ToString()
                    }),
                    "Value", "Text");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting create coupon view");
                return View();
            }
        }

        // POST: Admin/Coupons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CouponString,DiscountType,DiscountAmount,StartPeriod,EndPeriod,Title,Description")] Coupon coupon, IFormFile? image)
        {
            ViewBag.DiscountType = new SelectList(
                Enum.GetValues(typeof(DiscountType))
                .Cast<DiscountType>()
                .Select(dt => new
                {
                    Value = dt,
                    Text = dt.ToString()
                }),
                "Value", "Text");
            
            try
            {
                if (ModelState.IsValid)
                {
                    coupon.IsActive = true;
                    
                    // Ensure Description is not null to prevent SQL error
                    if (string.IsNullOrEmpty(coupon.Description))
                    {
                        coupon.Description = string.Empty;
                    }
                    if (string.IsNullOrEmpty(coupon.Title))
                    {
                        coupon.Title = string.Empty;
                    }

                    if (image != null)
                    {
                        coupon.ImageUrl = await coupon.UploadImage(image);
                    }

                    _context.Add(coupon);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                
                return View(coupon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating coupon");
                ModelState.AddModelError("", "An error occurred while saving the coupon. Please check all required fields.");
                return View(coupon);
            }
        }

        // GET: Admin/Coupons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound("Id is null");
                }

                var coupon = await _context.Coupons.FindAsync(id);
                if (coupon == null)
                {
                    return NotFound($"Coupon with id {id} not found");
                }
                ViewBag.DiscountType = new SelectList(
                    Enum.GetValues(typeof(DiscountType))
                    .Cast<DiscountType>()
                    .Select(dt => new
                    {
                        Value = dt,
                        Text = dt.ToString()
                    }),
                        "Value", "Text");
                return View(coupon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing coupon");
                return NotFound($"Error editing coupon: {ex.Message}");
            }
        }

        // POST: Admin/Coupons/Edit/Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CouponString,DiscountType,DiscountAmount,StartPeriod,EndPeriod,IsActive")] Coupon coupon)
        {
            try
            {

                if (id != coupon.Id)
                {
                    return NotFound($"Coupon with id {id} not found");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            _context.Update(coupon);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return RedirectToAction(nameof(Index));
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        _logger.LogError(ex, "Error editing coupon");

                        if (!CouponExists(coupon.Id))
                        {
                            return NotFound($"Coupon with id {coupon.Id} not found");
                        }
                        else
                        {
                            _logger.LogError(ex, "Error editing coupon");
                            throw;
                        }
                    }
                }

                ViewBag.DiscountType = new SelectList(
                    Enum.GetValues(typeof(DiscountType))
                    .Cast<DiscountType>()
                    .Select(dt => new
                    {
                        Value = dt,
                        Text = dt.ToString()
                    }),
                    "Value", "Text");

                return View(coupon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing coupon");
                return View(coupon);
            }
        }

        // GET: Admin/Coupons/Delete/Id
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (coupon == null)
                {
                    return NotFound($"Coupon with id {id} not found");
                }

                return View(coupon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon");
                return NotFound($"Error deleting coupon: {ex.Message}");
            }
        }

        // POST: Admin/Coupons/Delete/Id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var coupon = await _context.Coupons.FindAsync(id);
                if (coupon != null)
                {
                    _context.Coupons.Remove(coupon);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return NotFound($"Coupon with id {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon");
                return RedirectToAction(nameof(Index));
            }
        }

        private bool CouponExists(int id)
        {
            return _context.Coupons.Any(e => e.Id == id);
        }
    }
}
