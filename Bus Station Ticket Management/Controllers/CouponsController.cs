using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;

namespace Bus_Station_Ticket_Management.Controllers
{
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Coupons
        public async Task<IActionResult> Index()
        {
            return View(await _context.Coupons.ToListAsync());
        }

        // POST: Coupons/Apply
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(int tripId, string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode)) {
                TempData["Error"] = "Vui lòng nhập mã giảm giá.";
                return RedirectToAction("SelectSeats", "Trips", new { id = tripId });
            }

            var now = DateTime.Now;
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.CouponString == couponCode && c.StartPeriod <= now && c.EndPeriod >= now);

            //if (coupon == null) {
            //    TempData["Error"] = "Mã giảm giá không hợp lệ hoặc đã hết hạn.";
            //}
            //else {
            //    TempData["Success"] = $"Áp dụng mã thành công! Giảm {coupon.DiscountAmount:N0}đ.";
            //    TempData["CouponCode"] = coupon.CouponString;
            //    TempData["DiscountAmount"] = coupon.DiscountAmount;
            //}

            return RedirectToAction("SelectSeats", "Trips", new { id = tripId });
        }

    }
}
