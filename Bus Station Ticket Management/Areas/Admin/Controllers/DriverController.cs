using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]

    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DriverController> _logger;

        public DriverController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<DriverController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Driver
        public async Task<IActionResult> Index()
        {
            try
            {
                var drivers = await _context.Drivers
                    .Include(d => d.Account).ToListAsync();

                return View(drivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // GET: Driver/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers.Include(d => d.Account).FirstOrDefaultAsync(m => m.Id == id);
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        public async Task<IActionResult> DetailsPartial(string? id)
        {
            try {
                
                if (id == null)
                {
                    return NotFound();
                }

                var driver = await _context.Drivers
                    .Include(d => d.Account)
                    .Include(d => d.DriverLicenses)
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (driver == null)
                {
                    return NotFound();
                }

                var viewmodel = new DriverViewModel
                {
                    DriverId = driver.Id,
                    FullName = driver.Account?.FullName,
                    Email = driver.Account?.Email,
                    PhoneNumber = driver.Account?.PhoneNumber,
                    DateOfBirth = driver.Account?.DateOfBirth,
                    Gender = driver.Account?.Gender,
                    Address = driver.Account?.Address,
                    Avatar = driver.Account?.Avatar,
                    Licenses = driver.DriverLicenses?.Select(l => new DriverLicenseViewModel
                    {
                        LicenseId = l.LicenseId,
                        LicenseClass = l.LicenseClass,
                        LicenseIssueDate = l.LicenseIssueDate,
                        LicenseExpirationDate = l.LicenseExpirationDate,
                        LicenseIssuePlace = l.LicenseIssuePlace
                    }).ToList() ?? []
                };

                return PartialView("_DetailsPartial", viewmodel);
            } catch (Exception ex) {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // GET: Driver/Create
        public IActionResult Create()
        {
            var viewmodel = new DriverViewModel
            {
                Licenses = new List<DriverLicenseViewModel> { new DriverLicenseViewModel() }
            };
            return View(viewmodel);
        }

        private bool IsDriver(DriverViewModel model)
        {
            if (model.DateOfBirth.HasValue && model.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future");
                return false;
            }
            if (model.Licenses == null)
            {
                ModelState.AddModelError("Licenses", "Licenses are required");
                return false;
            }

            if (model.Licenses.Any(l => l.LicenseIssueDate == null))
            {
                ModelState.AddModelError("LicenseIssueDate", "License issue date is required");
                return false;
            }

            // Set expiration date to a far future date if not provided
            foreach (var license in model.Licenses)
            {
                if (license.LicenseExpirationDate < license.LicenseIssueDate)
                {
                    ModelState.AddModelError("LicenseExpirationDate", "License expiration date cannot be before issue date");
                    return false;
                }
            }

            if (model.Licenses.Any(l => l.LicenseIssuePlace == null))
            {
                ModelState.AddModelError("LicenseIssuePlace", "License issue place is required");
                return false;
            }

            return true;
        }

        // POST: Driver/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DriverViewModel model, IFormFile avatar, string password)
        {
            try
            {
                if (model == null)
                {
                    return View(new DriverViewModel());
                }

                if (!IsDriver(model))
                {
                    return View(model);
                }

                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage;
                    if (!string.IsNullOrEmpty(firstError))
                    {
                        ModelState.AddModelError(string.Empty, firstError);
                    }
                    return View(model);
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        model.Avatar = await ApplicationUser.UploadAvatar(avatar);

                        var user = new ApplicationUser
                        {
                            UserName = model.Email,
                            Email = model.Email,
                            FullName = model.FullName,
                            Address = model.Address,
                            DateOfBirth = model.DateOfBirth,
                            Gender = model.Gender,
                            PhoneNumber = model.PhoneNumber,
                            Avatar = model.Avatar
                        };

                        var result = await _userManager.CreateAsync(user, password);
                        if (!result.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(model);
                        }

                        result = await _userManager.AddToRoleAsync(user, "Driver");
                        if (!result.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(model);
                        }

                        if (model.Licenses == null || model?.Licenses?.Count <= 0)
                        {
                            await transaction.RollbackAsync();
                            ModelState.AddModelError(string.Empty, "At least one license is required.");
                            return View(model);
                        }
                        
                        var userId = user.Id;

                        var driver = new Driver {
                            Id = userId,
                            Account = user
                        };

                        _context.Drivers.Add(driver);

                        foreach (var license in model.Licenses)
                        {
                            var driverLicense = new DriverLicense
                            {
                                DriverId = userId,
                                LicenseId = license.LicenseId,
                                LicenseClass = license.LicenseClass,
                                LicenseIssueDate = license.LicenseIssueDate,
                                LicenseExpirationDate = license.LicenseExpirationDate,
                                LicenseIssuePlace = license.LicenseIssuePlace
                            };

                            _context.DriverLicenses.Add(driverLicense);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"{ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Unexpected error: {ex.Message}");
                return View(model);
            }
        }

        // GET: Driver/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            try {
                if (id == null)
                {
                    return NotFound();
                }

                var driver = await _context.Drivers
                    .Include(d => d.Account)
                    .Include(d => d.DriverLicenses)
                    .FirstOrDefaultAsync(d => d.Id == id);
                    
                if (driver == null)
                {
                    return NotFound();
                }

                var viewmodel = new DriverViewModel
                {
                    DriverId = driver.Id,
                    FullName = driver.Account?.FullName,
                    Email = driver.Account?.Email,
                    PhoneNumber = driver.Account?.PhoneNumber,
                    DateOfBirth = driver.Account?.DateOfBirth,
                    Gender = driver.Account?.Gender,
                    Address = driver.Account?.Address,
                    Avatar = driver.Account?.Avatar,
                    Licenses = driver.DriverLicenses?.Select(l => new DriverLicenseViewModel
                    {
                        LicenseId = l.LicenseId,
                        LicenseClass = l.LicenseClass,
                        LicenseIssueDate = l.LicenseIssueDate,
                        LicenseExpirationDate = l.LicenseExpirationDate,
                        LicenseIssuePlace = l.LicenseIssuePlace
                    }).ToList() ?? []
                };
                return View(viewmodel);
            } catch (Exception ex) {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // POST: Driver/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, DriverViewModel model, IFormFile Avatar, string? password)
        {
            if (id == null || model.DriverId != id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                var driver = await _context.Drivers.FindAsync(id);
                if (driver == null || driver.Id == null)
                {
                    return NotFound();
                }

                var user = await _userManager.FindByIdAsync(driver.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.Email;
                user.FullName = model.FullName;
                user.Address = model.Address;
                user.Gender = model.Gender;
                user.DateOfBirth = model.DateOfBirth;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Avatar = await ApplicationUser.UploadAvatar(Avatar);

                if (!string.IsNullOrEmpty(password) && password.Length > 0 && !string.IsNullOrWhiteSpace(password)) {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, password);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        await transaction.RollbackAsync();
                        return View(model);
                    }
                }

                _context.Update(driver);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!DriverExists(model.DriverId))
                {
                    return NotFound(ex.ToString());
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // GET: Driver/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            try {
                if (id == null)
                {
                    return NotFound("Id not found");
                }

                var driver = await _context.Drivers
                    .Include(d => d.Account)
                    .Include(d => d.DriverLicenses)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (driver == null)
                {
                    return NotFound("Driver not found");
                }

                return View(driver);

            } catch (Exception ex) {
                _logger.LogError(ex.ToString(), ex);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // POST: Driver/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try {
                var driver = await _context.Drivers.Include(d => d.DriverLicenses).FirstOrDefaultAsync(d => d.Id == id);

                if (driver == null)
                {
                    return NotFound();
                }

                using (var transaction = await _context.Database.BeginTransactionAsync()) {
                    try {
                        if (driver.DriverLicenses != null) {
                            _context.DriverLicenses.RemoveRange(driver.DriverLicenses);
                        }
                        
                        _context.Drivers.Remove(driver);
                        
                        await ApplicationUser.DeleteAvatar(driver.Account?.Avatar);
                        
                        var user = await _userManager.FindByIdAsync(driver.Id);
                        if (user != null)
                        {
                            var result = await _userManager.DeleteAsync(user);
                            if (!result.Succeeded)
                            {
                                await transaction.RollbackAsync();
                                foreach (var error in result.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                return View(driver);
                            }
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return RedirectToAction(nameof(Index));
                    } catch (Exception ex) {
                        await transaction.RollbackAsync();
                        throw new Exception($"Failed to delete driver: {ex}");
                    }
                }
            } catch (Exception ex) {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        private bool DriverExists(string id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }
    }
}