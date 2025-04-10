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
		public IActionResult ApplyCoupon(string couponCode, int tripId)
		{
			var coupon = _context.Coupons.FirstOrDefault(c => c.CouponString == couponCode);
			var now = DateTime.Now;

			if (!(coupon != null && now >= coupon.StartPeriod && now <= coupon.EndPeriod && coupon.IsActive == true))
			{
				return Json(new { success = false, message = "Invalid or expired coupon." });	
			}
			// Coupon is valid and active

			if (coupon.StartPeriod > now || coupon.EndPeriod < now)
			{
				if (coupon.IsActive == true) 
				{
					coupon.IsActive = false;
					_context.SaveChangesAsync();
				}
				return Json(new { success = false, message = "Coupon is not valid for this period." });
			}	

			// Get trip price (assuming one seat)
			var trip = _context.Trips.Find(tripId);
			if (trip == null)
			{
				return Json(new { success = false, message = "Trip not found." });
			}

			decimal discountAmount = 0;

			if (coupon.DiscountType == DiscountType.Percentage)
			{
				discountAmount = trip.TotalPrice * (coupon.DiscountAmount / 100);
			}
			else
			{
				discountAmount = coupon.DiscountAmount;
			}

			return Json(new
			{
				success = true,
				discountedAmount = discountAmount,
				couponType = coupon.DiscountType.ToString(),
				discountValue = coupon.DiscountAmount,
				message = $"Coupon applied! You saved {(coupon.DiscountType == DiscountType.Percentage ? $"{coupon.DiscountAmount}%" : $"{discountAmount:N0}đ")}."
			});
		}
	}
}
