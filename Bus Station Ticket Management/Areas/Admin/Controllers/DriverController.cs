using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Bus_Station_Ticket_Management.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Collections;
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
        private readonly SignInManager<ApplicationUser> _signInManager;

        public DriverController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<DriverController> logger, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
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
            try
            {

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
            }
            catch (Exception ex)
            {
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
        public async Task<IActionResult> Create([Bind("FullName,Email,PhoneNumber,DateOfBirth,Gender,Address,Licenses")] DriverViewModel model, IFormFile avatar, string password)
        {
            try
            {
                var validationResult = ValidateDriverModel(model);
                if (!validationResult)
                {
                    return View(model);
                }

                foreach (var license in model.Licenses)
                {
                    if (license.hasExpDate)
                    {
                        if (license.LicenseExpirationDate < license.LicenseIssueDate)
                        {
                            ModelState.AddModelError("LicenseExpirationDate", "License expiration date cannot be before issue date");
                            return View(model);
                        }
                    }
                    else
                    {
                        license.LicenseExpirationDate = DateOnly.MaxValue; // Set to a far future date if not provided
                    }
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);

                if (existingUser != null)
                {
                    if (await _userManager.IsInRoleAsync(existingUser, "Driver"))
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                        return View(model);
                    }
                    else
                    {
                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                await HandleExistingDriver(existingUser, model, avatar, password);
                                await transaction.CommitAsync();
                                TempData["SuccessMessage"] = "Driver created successfully.";
                                return RedirectToAction(nameof(Index));
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogError(ex, "Error in driver creation transaction");
                                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                                return View(model);
                            }
                        }
                    }
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await HandleCreateNewDriver(model, avatar, password);
                        await transaction.CommitAsync();
                        TempData["SuccessMessage"] = "Driver created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error in driver creation transaction");
                        ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                        return View(model);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in driver creation");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        public async Task HandleExistingDriver(ApplicationUser existingUser, DriverViewModel model, IFormFile avatar, string password)
        {
            try
            {
                // If user exists but is not a driver, add them to the Driver role
                var result = await _userManager.AddToRoleAsync(existingUser, "Driver");
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    throw new InvalidOperationException("Failed to add user to Driver role");
                }

                existingUser.FullName = model.FullName;
                existingUser.Address = model.Address;
                existingUser.DateOfBirth = model.DateOfBirth;
                existingUser.Gender = model.Gender;
                existingUser.PhoneNumber = model.PhoneNumber;

                var loginResult = await _signInManager.PasswordSignInAsync(existingUser, password, true, lockoutOnFailure: false);
                if (!loginResult.Succeeded)
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                            await _userManager.ResetPasswordAsync(existingUser, token, password);
                            await transaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Error in driver creation transaction");
                            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                            throw new InvalidOperationException("Failed to update user information");
                        }
                    }
                }

                // Handle avatar upload if provided
                if (avatar != null)
                {
                    try
                    {
                        existingUser.Avatar = await ApplicationUser.UploadAvatar(avatar);
                        if (existingUser.Avatar == null)
                        {
                            throw new InvalidOperationException("Failed to upload avatar");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading avatar");
                        throw new InvalidOperationException("Failed to upload avatar");
                    }
                }

                var driver = new Driver
                {
                    Id = existingUser.Id,
                    Account = existingUser
                };

                await _context.Drivers.AddAsync(driver);

                UpdateDriverLicenses(driver, model.Licenses);

                await _context.DriverLicenses.AddRangeAsync(driver.DriverLicenses);

                result = await _userManager.UpdateAsync(existingUser);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    throw new InvalidOperationException("Failed to update user information");
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Driver updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in driver creation transaction");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                throw new InvalidOperationException("Failed to update user information");
            }
        }

        private void UpdateDriverLicenses(Driver driver, IEnumerable<DriverLicenseViewModel> model)
        {
            if (driver.DriverLicenses != null)
            {
                _context.DriverLicenses.RemoveRange(driver.DriverLicenses);
            }

            driver.DriverLicenses = [.. model.Select(l => new DriverLicense
            {
                DriverId = driver.Id,
                LicenseId = l.LicenseId,
                LicenseClass = l.LicenseClass,
                LicenseIssueDate = l.LicenseIssueDate,
                LicenseExpirationDate = l.LicenseExpirationDate,
                LicenseIssuePlace = l.LicenseIssuePlace
            })];
        }

        public async Task HandleCreateNewDriver(DriverViewModel model, IFormFile avatar, string password)
        {
            // Handle avatar upload first
            string? avatarPath = null;
            if (avatar != null)
            {
                try
                {
                    avatarPath = await ApplicationUser.UploadAvatar(avatar);
                    if (avatarPath == null)
                    {
                        throw new Exception("Failed to upload avatar");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading avatar");
                    ModelState.AddModelError("Avatar", "Failed to upload avatar. Please try again.");
                    throw new InvalidOperationException("Failed to upload avatar");
                }
            }

            // Create user first
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                PhoneNumber = model.PhoneNumber,
                Avatar = avatarPath
            };

            // Create user with password
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                throw new InvalidOperationException("Failed to create user");
            }

            // Add to Driver role
            result = await _userManager.AddToRoleAsync(user, "Driver");
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                throw new InvalidOperationException("Failed to add user to Driver role");
            }

            // Validate licenses
            if (model.Licenses == null || !model.Licenses.Any())
            {
                ModelState.AddModelError(string.Empty, "At least one license is required.");
                throw new InvalidOperationException("At least one license is required.");
            }

            // Create driver record
            var driver = new Driver
            {
                Id = user.Id,
                Account = user
            };

            await _context.Drivers.AddAsync(driver);

            // Add licenses
            UpdateDriverLicenses(driver, model.Licenses);

            await _context.DriverLicenses.AddRangeAsync(driver.DriverLicenses);

            await _context.SaveChangesAsync();
        }

        // GET: Driver/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null || !DoesDriverExists(id))
            {
                return NotFound("Driver not found");
            }
            try
            {
                var driver = await _context.Drivers
                    .Include(d => d.Account)
                    .Include(d => d.DriverLicenses)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (driver == null)
                {
                    return NotFound("No driver found");
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
                    RowVersion = driver.RowVersion,
                    Licenses = driver.DriverLicenses?.Select(l => new DriverLicenseViewModel
                    {
                        LicenseId = l.LicenseId,
                        LicenseClass = l.LicenseClass,
                        LicenseIssueDate = l.LicenseIssueDate,
                        LicenseExpirationDate = l.LicenseExpirationDate,
                        LicenseIssuePlace = l.LicenseIssuePlace
                    }).ToList() ?? []
                };
                ViewData["Id"] = id;

                return View(viewmodel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // POST: Driver/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, DriverViewModel model, IFormFile? Avatar, string? password)
        {
            ViewData["Id"] = id;

            var validationResult = ValidateDriverModel(model, id);
            if (!validationResult)
            {
                return View(model);
            }

            foreach (var license in model.Licenses)
            {
                if (license.hasExpDate)
                {
                    if (license.LicenseExpirationDate < license.LicenseIssueDate)
                    {
                        ModelState.AddModelError("LicenseExpirationDate", "License expiration date cannot be before issue date");
                        return View(model);
                    }
                }
                else
                {
                    license.LicenseExpirationDate = DateOnly.MaxValue; // Set to a far future date if not provided
                }
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Get driver with concurrency token
                    var driver = await _context.Drivers
                        .Include(d => d.Account)
                        .Include(d => d.DriverLicenses)
                        .FirstOrDefaultAsync(d => d.Id == id);

                    if (driver == null)
                    {
                        return NotFound();
                    }

                    // Check if the record has been modified by another user
                    if (!StructuralComparisons.StructuralEqualityComparer.Equals(driver.RowVersion, model.RowVersion))
                    {
                        ModelState.AddModelError(string.Empty, "The record was modified by another user. Please refresh and try again.");
                        return View(model);
                    }

                    // Check if email is being changed and if it's already in use
                    var user = await _userManager.FindByIdAsync(driver.Id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    if (user.Email != model.Email)
                    {
                        var existingUser = await _userManager.FindByEmailAsync(model.Email);
                        if (existingUser != null && existingUser.Id != user.Id)
                        {
                            ModelState.AddModelError("Email", "This email is already registered.");
                            return View(model);
                        }
                    }

                    try
                    {
                        // Handle avatar upload if provided
                        if (Avatar != null)
                        {
                            try
                            {
                                var newAvatarPath = await ApplicationUser.UploadAvatar(Avatar);
                                // Delete old avatar if exists
                                if (!string.IsNullOrEmpty(user.Avatar))
                                {
                                    await ApplicationUser.DeleteAvatar(user.Avatar);
                                }
                                user.Avatar = newAvatarPath;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error uploading avatar");
                                ModelState.AddModelError("Avatar", "Failed to upload avatar. Please try again.");
                                return View(model);
                            }
                        }

                        // Update user properties
                        user.UserName = model.Email;
                        user.FullName = model.FullName;
                        user.Address = model.Address;
                        user.Gender = model.Gender;
                        user.DateOfBirth = model.DateOfBirth;
                        user.Email = model.Email;
                        user.PhoneNumber = model.PhoneNumber;

                        // Update password if provided
                        if (!string.IsNullOrEmpty(password) && password.Length > 0 && !string.IsNullOrWhiteSpace(password))
                        {
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

                        // Update user
                        var updateResult = await _userManager.UpdateAsync(user);
                        if (!updateResult.Succeeded)
                        {
                            foreach (var error in updateResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            await transaction.RollbackAsync();
                            return View(model);
                        }

                        // Update licenses
                        if (model.Licenses != null && model.Licenses.Count > 0)
                        {
                            UpdateDriverLicenses(driver, model.Licenses);
                        }

                        try
                        {
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            TempData["SuccessMessage"] = "Driver updated successfully.";
                            return RedirectToAction(nameof(Index));
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Concurrency error while updating driver");
                            ModelState.AddModelError(string.Empty, "The record was modified by another user. Please refresh and try again.");
                            return View(model);
                        }
                        catch (DbUpdateException ex)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Database error while updating driver");
                            ModelState.AddModelError(string.Empty, "An error occurred while saving the changes. Please try again.");
                            return View(model);
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error in driver update transaction");
                        ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Unexpected error in driver update");
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                    return View(model);
                }
            }
        }

        // GET: Driver/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            try
            {
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

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString(), ex);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // POST: Driver/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var driver = await _context.Drivers.Include(d => d.DriverLicenses).FirstOrDefaultAsync(d => d.Id == id);

                if (driver == null)
                {
                    return NotFound();
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        if (driver.DriverLicenses != null)
                        {
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
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"Failed to delete driver: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        private bool DoesDriverExists(string id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }

        private bool ValidateDriverModel(DriverViewModel model, string? id = null)
        {
            if (model == null)
            {
                return false;
            }

            if (!IsDriver(model))
            {
                return false;
            }

            if (id != null && !DoesDriverExists(id))
            {
                ModelState.ClearValidationState("id");
            }

            // Validate model state
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage;
                if (!string.IsNullOrEmpty(firstError))
                {
                    ModelState.AddModelError(string.Empty, firstError);
                }
                return false;
            }

            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError("Email", "Email is required");
                return false;
            }

            // Only validate avatar in Create mode
            var isCreateAction = HttpContext.Request.Path.Value?.EndsWith("/Create", StringComparison.OrdinalIgnoreCase) ?? false;
            if (isCreateAction && string.IsNullOrEmpty(model.Avatar))
            {
                ModelState.AddModelError("Avatar", "Avatar is required");
                return false;
            }

            return true;
        }
    }
}