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
using X.PagedList.Extensions;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]

    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Coupons
        public async Task<IActionResult> Index()
        {
            var coupons = await _context.Coupons.ToListAsync();
            return View(coupons);
        }


        // GET: Admin/Coupons/Details/5
        public async Task<IActionResult> Details(int? id)
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

        public async Task<IActionResult> DetailsPartial(int? id)
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

        // GET: Admin/Coupons/Create
        public IActionResult Create()
        {
            ViewBag.DiscountType = new SelectList(
                Enum.GetValues(typeof (DiscountType))
                .Cast<DiscountType>()
                .Select(dt => new {
                    Value = dt,
                    Text = dt.ToString()
                }),
                "Value", "Text");
            return View();
        }

        // POST: Admin/Coupons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CouponString,DiscountType,DiscountAmount,StartPeriod,EndPeriod")] Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                coupon.IsActive = true;
                _context.Add(coupon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // ViewBag.DiscountType = new SelectList(Enum.GetValues(typeof (DiscountType))
            //     .Cast<DiscountType>()
            //     .Select(dt => new {
            //         Value = dt,
            //         Text = dt.ToString()
            //     }),
            //     "Value", "Text");
            return View(coupon);
        }

        // GET: Admin/Coupons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }
            ViewBag.DiscountType = new SelectList(
                Enum.GetValues(typeof (DiscountType))
                .Cast<DiscountType>()
                .Select(dt => new {
                    Value = dt,
                    Text = dt.ToString()
                }),
                "Value", "Text");
            return View(coupon);
        }

        // POST: Admin/Coupons/Edit/Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CouponString,DiscountType,DiscountAmount,StartPeriod,EndPeriod,IsActive")] Coupon coupon)
        {
            if (id != coupon.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(coupon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CouponExists(coupon.Id))
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
            ViewBag.DiscountType = new SelectList(
                Enum.GetValues(typeof (DiscountType))
                .Cast<DiscountType>()
                .Select(dt => new {
                    Value = dt,
                    Text = dt.ToString()
                }),
                "Value", "Text");
            return View(coupon);
        }

        // GET: Admin/Coupons/Delete/Id
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Admin/Coupons/Delete/Id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CouponExists(int id)
        {
            return _context.Coupons.Any(e => e.Id == id);
        }
    }
}
